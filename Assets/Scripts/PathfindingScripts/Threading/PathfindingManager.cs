using System.Collections;
using System.Collections.Generic;
using System.Text;
using ThreadedPathfinding;
using ThreadedPathfinding.Internal;
using UnityEngine;
using Encounter;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = GameObject.FindObjectOfType<PathfindingManager>();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
    private static PathfindingManager _instance;

    public TileProvider Provider
    {
        get
        {
            return _provider;
        }
        set
        {
            _provider = value;
        }
    }
    private TileProvider _provider;

    [Header("Startup")]
    public bool AutoCreateThreads = true;
    public bool AutoStartThreads = true;

    [Header("Threading Settings")]
    [Range(1, 16)]
    public int ThreadCount = 2;

    [Header("Debug")]
    [TextArea(20, 20)]
    public string Info;

    public object QueueLock = new object(); // The lock for adding to and removing from the processing queue.
    public object ReturnLock = new object(); // The lock for adding to and removing from the return queue.
    private PathfindingThread[] threads;
    private List<PathfindingRequest> pending = new List<PathfindingRequest>();
    private List<PathfindingResolution> returns = new List<PathfindingResolution>();
    private List<PathfindingResolution> events = new List<PathfindingResolution>();
    private StringBuilder str = new StringBuilder();
    private ThreadedPathfinding.Internal.Pathfinding Pathfinder = new ThreadedPathfinding.Internal.Pathfinding();

    public void Awake()
    {
        Instance = this;

        if (AutoCreateThreads)
        {
            // Create threads...
            CreateThreads(ThreadCount);
            if (AutoStartThreads)
            {
                // Run those threads.
                StartThreads();
            }
        }
    }

    public void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (threads != null)        
            StopThreads(); 
    }

    /// <summary>
    /// Should only be called from unity events such as Start or Update.
    /// The requests are actually scheduled at the end of LateUpdate.
    /// Requests are returned (once completed) before Update.
    /// For example, a request is made in frame 0 in the Update thread. The path takes 1 frame to calculate.
    /// Then, in frame 2, before any calls to Update, the return method is called.
    /// </summary>
    /// <param name="request"></param>
    public void Enqueue(PathfindingRequest request)
    {
        if (request == null)
            return;

        if (!pending.Contains(request))
        {
            pending.Add(request);
        }
        else
        {
            Debug.LogWarning("That pathfinding request was already submitted.");
        }
    }

    public void AddResponse(PathfindingResolution pending, PathfindingRequest request)
    {
        // Prevent this from being called when currently returning stuff.
        lock (ReturnLock)
        {
            if (pending == null)
            {
                throw new System.Exception("Invalid parameter: PathfindingResolution in AddResponse was null");
            }
            if (request == null)
            {
                throw new System.Exception("Invalid parameter: PathfindingRequest in AddResponse was null");
            }
            if (pending.Callback == null)
            {
                returns.Add(pending);
                request.Dispose();
            }
            else
            {
                events.Add(pending);
            }
        }
    }
    public List<PathNode> SyncFindPath(int startX, int startY, int endX, int endY, PathfindingConfig pconf)
    {
        List<PNode> nodes = new List<PNode>();
        PathfindingResult result = Pathfinder.Run(startX, startY, endX, endY, pconf, Provider, null, out nodes);
        return Provider.TranslatePath(nodes);
    }
    public IEnumerator GetPath(int startX, int startY, int endX, int endY, PathfindingConfig pconf, PathFound voider = null)
    {
        if (startX == endX && startY == endY)
            return null; // To be changed to a better response

        // Create the request, with the callback method being UponPathCompleted.
        PathfindingRequest request = PathfindingRequest.Create(startX, startY, endX, endY, pconf, voider);

        return WaitFor(request);
    }

    private PathfindingResolution CheckReturnWithRequest(PathfindingRequest req)
    {
        lock(ReturnLock)
        {
            return returns.Find(res => {
                if (req == null)
                {
                    Debug.LogWarning("Logic warning: PathfindingRequest died before it found its resolution");
                    return false;
                }
                if (res == null)
                {
                    Debug.LogWarning("Logic warning: PathfindingResolution in returns was null");
                    return false;
                }
                return res.Id == req.Id;
            });
        }
    }
    public IEnumerator WaitFor(PathfindingRequest req)
    {
        int counter = 0;
        PathfindingResolution foundRes;
        while ((foundRes = CheckReturnWithRequest(req)) == null && counter < 100)
        {
            counter += 1;
            if (counter % 10 == 0) Debug.Log($"Did not find resolution... {counter}, Id: {req.Id}");
            yield return null;
        }
        if (counter >= 100)
        {
            Debug.Log($"Timed out, no path found...");
            Debug.Log($"No resolution with id: {req.Id}");
            yield return null;
        }
        else
        {
            //Debug.Log($"Found resolution with id: {foundRes.Id} and path length {foundRes.Path?.Count} and result {foundRes.Result}");
            returns.Remove(foundRes);
            yield return foundRes;
        }
    }

    public void Update()
    {
        // Assumes that this is called before all other script's update calls. It still works regardless of order but
        // it makes more sense to give path requests back before the update.

        lock (ReturnLock)
        {
            foreach (var item in events)
            {
                if(item.Callback != null)
                {
                    item.Callback.Invoke(item.Result, item.Path);
                }
                else
                {
                    // Object must have been destroyed...
                }
            }
            events.Clear();
            int len = returns.Count;
            var deadItems = new List<PathfindingResolution>();
            foreach (var item in returns)
            {
                if (item == null)
                {

                    Debug.Log($"Removed null");
                    deadItems.Add(item);
                    continue;
                }
                item.FramesToLive--;
                if (item.FramesToLive < 0)
                {
                    deadItems.Add(item);
                }
                if (returns.Count == 1)
                {
                    Debug.Log($"The id of this item: {item.Id}");
                    Debug.Log($"This is the number of nulls waiting in returns: {returns.FindAll(r => r==null).Count}");
                }
            }
            foreach(PathfindingResolution res in deadItems)
            {
                Debug.Log($"res {res?.Id} with path length {res?.Path?.Count} failed with result {res?.Result}");
                returns.Remove(res);
            }
            deadItems.Clear();
        }
    }

    public void LateUpdate()
    {
        if (threads == null || threads.Length == 0)
            return;

# if UNITY_EDITOR
        // Compile debug information.
        str.Append("Overall work strain: ");
        float sum = 0f;
        for (int i = 0; i < threads.Length; i++)
        {
            float w = threads[i].AproximateWork;
            sum += w;
        }
        sum /= threads.Length;
        str.Append((sum * 100f).ToString("N0"));
        str.Append("%");
        str.AppendLine();
        str.AppendLine();
        str.Append("Requests this frame: ");
        str.Append(pending.Count);
        str.AppendLine();
        str.Append("Pending returns: ");
        str.Append(returns.Count);
        str.AppendLine();
        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            int count = t.Queue.Count;
            long time = t.LatestTime;
            float work = t.AproximateWork;

            str.Append("Thread #");
            str.Append(i);
            str.Append(": ");
            str.AppendLine();
            str.Append("  -");
            str.Append(count);
            str.Append(" pending requests.");
            str.AppendLine();
            str.Append("  -Last path time: ");
            str.Append(time);
            str.Append("ms");
            str.AppendLine();
            str.Append("  -Aprox. work: ");
            str.Append((work * 100f).ToString("N0"));
            str.Append("%");
            str.AppendLine();
        }

        this.Info = str.ToString();
        str.Length = 0;
#endif

        // Lock because the number of pending requests for each thread should not change.
        // Hopefully this lock should last no more than a millisecond at most, probably less.
        lock (QueueLock)
        {
            foreach (var request in pending)
            {
                // For each request we need to find the thread with the lowest number of requests on it.
                int lowest = int.MaxValue;
                PathfindingThread t = null;
                foreach (var thread in threads)
                {
                    if(thread.Queue.Count < lowest)
                    {
                        lowest = thread.Queue.Count;
                        t = thread;
                    }
                }

                // Enqueue it on this thread.
                t.Queue.Enqueue(request);
            }
            pending.Clear();
        }       
    }

    public void CreateThreads(int number)
    {
        if (threads != null)
            return;

        threads = new PathfindingThread[number];
        for (int i = 0; i < number; i++)
        {
            threads[i] = new PathfindingThread(this, i);
        }
    }

    public void StartThreads()
    {
        if (threads == null)
            return;

        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            t.StartThread();
        }
    }

    public void StopThreads()
    {
        if (threads == null)
            return;

        for (int i = 0; i < threads.Length; i++)
        {
            var t = threads[i];
            t.StopThread();
        }
    }
}
