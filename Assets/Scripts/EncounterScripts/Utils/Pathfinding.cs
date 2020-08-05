/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;
using UnityEngine.Profiling;

namespace Encounter
{
    public class Pathfinding : MonoBehaviour
    {
        private MapManager mgm;
        private GlobalEventManager gem;

        private List<Vector2Int> directions;

        private MapGrid grid;
        private List<PathNode> openList;
        private List<PathNode> closedList;
        private RouteCache cache = new RouteCache();


        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }

            directions = new List<Vector2Int>();
            directions.Add(new Vector2Int(1, 0));
            directions.Add(new Vector2Int(0, 1));
            directions.Add(new Vector2Int(0, -1));
            directions.Add(new Vector2Int(-1, 0));

            gem.StartListening("Move", ClearCache);
            gem.StartListening("Death", ClearCache);
        }

        void Start()
        {
            grid = mgm.grid;
        }
        private void Update()
        {
            if(Input.GetKeyDown("l"))
            {
                Debug.Log("counterPathCalls");
                Debug.Log(counterPathCalls);
                Debug.Log("counterCacheHit");
                Debug.Log(counterCacheHit);
                Debug.Log("counterCacheMiss");
                Debug.Log(counterCacheMiss);
            }
        }

        private class RouteCache
        {
            // from, to->list
            private Dictionary<PathNode, Dictionary<PathNode, List<PathNode>>> routeCache = new Dictionary<PathNode, Dictionary<PathNode, List<PathNode>>>();

            public void Put(PathNode start, PathNode end, List<PathNode> route)
            {
                if (routeCache.ContainsKey(start))
                {
                    routeCache[start].Add(end, route);
                }
                else
                {
                    routeCache.Add(start, new Dictionary<PathNode, List<PathNode>>() { { end, route } });
                }
            }

            public void CacheEntirePath(List<PathNode> path)
            {
                PathNode endNode = path.Last();
                //Debug.Log("endNode");
                //Debug.Log(endNode);
                for (int i = path.Count - 2; i >= 0; i--)
                {
                    //Debug.Log("i");
                    //Debug.Log(i);
                    //Debug.Log(path.Count);
                    PathNode current = path[i];

                    if (IsCached(current, endNode)) continue;

                    int length = path.Count - i;       // if i = 9, then length = 10 - 9, 1
                                                       // if i = 0, then length = 10 - 0, 10
                    Put(current, endNode, path.GetRange(i, length));
                }
            }

            public List<PathNode> Get(PathNode start, PathNode end)
            {
                if (IsCached(start, end)) return new List<PathNode>(routeCache[start][end]);
                else return new List<PathNode>();
            }

            public bool IsCached(PathNode start, PathNode end)
            {
                // check if route already cached
                if (!routeCache.ContainsKey(start)) return false;
                if (!routeCache[start].ContainsKey(end)) return false;
                return true;
            }
            public void Clear()
            {
                routeCache = new Dictionary<PathNode, Dictionary<PathNode, List<PathNode>>>();
            }
        }
        private void ClearCache(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            cache.Clear();
        }
        /// <summary>
        /// From Red Blob: I'm using an unsorted array for this example, but ideally this
        /// would be a binary heap.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class PriorityQueue<T>
        {
            private List<KeyValuePair<T, float>> elements = new List<KeyValuePair<T, float>>();

            public List<KeyValuePair<T, float>> GetUnderlyingList()
            {
                return elements;
            }

            public PriorityQueue() { }
            public PriorityQueue(T node, float priority)
            {
                Enqueue(node, priority);
            }
            public int Count
            {
                get { return elements.Count; }
            }

            public void Enqueue(T item, float priority)
            {
                elements.Add(new KeyValuePair<T, float>(item, priority));
            }

            // Returns the Location that has the lowest priority
            public T Dequeue()
            {
                int bestIndex = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Value < elements[bestIndex].Value)
                    {
                        bestIndex = i;
                    }
                }

                T bestItem = elements[bestIndex].Key;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }
        }

        /// <summary>
        /// Concatenate new part of the route with pre-cached route
        /// </summary>
        /// <param name="startNode"></param>
        /// <param name="endNode"></param>
        /// <param name="pathToCached"></param>
        /// <returns></returns>
        private List<PathNode> MergePathWithCache(PathNode startNode, PathNode endNode, List<PathNode> pathToCached)
        {
            PathNode startCachedNode = pathToCached.Last();
            //Debug.Log("this part of the path:[" + pathToCached.Last() + ", to:" + endNode + "]is already in cache.");
            //List<PathNode> newRoute = reconstructPath(parentMap, startNode, current);
            List<PathNode> cachedSubRoute = cache.Get(startCachedNode, endNode);

            pathToCached.RemoveAt(pathToCached.Count - 1);

            List<PathNode> mergedPath = pathToCached.Concat(cachedSubRoute).ToList();

            // remove last element
            // combine with cached route

            // cache the whole route
            //this.cache.put(startNode.getLocation(), goal, (LinkedList)newRoute);
            // return result
            return mergedPath;
        }

        public List<PathNode> FindPath(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }
            if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
            {
                return new List<PathNode>();
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;
            PathNode endNode = grid.GetGridObject(endX, endY).pn;

            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetGridObject(x, y).pn;
                    pathNode.gCost = 99999999;
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);
                if (currentNode == endNode)
                {
                    return CalculatePath(endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                List<PathNode> neighbours;


                neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

                Profiler.BeginSample("Pathfinding: FindPathInto2");
                foreach (PathNode neighbourNode in neighbours)
                {
                    if (closedList.Contains(neighbourNode)) continue;

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
                Profiler.EndSample();
            }
            // Out of nodes on the openList
            return new List<PathNode>();
        }
        PriorityQueue<PathNode> openQueue;
        List<PathNode> exploredNodes;
        int counterPathCalls;
        int counterCacheHit;
        int counterCacheMiss;
        public List<PathNode> FindPathInto3(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }
            if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
            {
                return new List<PathNode>();
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;
            PathNode endNode = grid.GetGridObject(endX, endY).pn;

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetGridObject(x, y).pn;
                    pathNode.path = new List<PathNode> { pathNode };
                    pathNode.gCost = 99999999;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);


            openQueue = new PriorityQueue<PathNode>(startNode, startNode.fCost);
            exploredNodes = new List<PathNode>();

            if (cache.IsCached(startNode, endNode))
            {
                Debug.Log("Found cache: cache.Get(startNode, endNode)");
                return cache.Get(startNode, endNode);
            }

            int timer = DateTime.Now.Millisecond;

            while (openQueue.Count > 0)
            {
                PathNode currentNode = openQueue.Dequeue();

                if (cache.IsCached(currentNode, endNode))
                {
                    Debug.Log("Found cache: MergePathWithCache");
                    return MergePathWithCache(startNode, endNode, currentNode.path);
                }

                Debug.Log(currentNode);

                if (currentNode == endNode)
                {
                    Debug.Log("Found current: CacheEntirePath");
                    cache.CacheEntirePath(currentNode.path);
                    return currentNode.path;
                }
                else
                {
                    foreach (PathNode adj in GetAdjacentNodes(endNode.x, endNode.y))
                    {
                        if (adj == currentNode)
                        {
                            Debug.Log("Found adjacent: CacheEntirePath");
                            List<PathNode> finalPath = currentNode.path;
                            finalPath.Add(endNode);
                            //finalPath.ForEach(Debug.Log);
                            cache.CacheEntirePath(finalPath);
                            return finalPath;
                        }
                    }
                }

                exploredNodes.Add(currentNode);

                List<PathNode> neighbours = GetNeighbourListFiltered(currentNode, pfconfig);
                Profiler.BeginSample("Pathfinding: FindPathInto2");
                Debug.Log("Neighbours!");
                //neighbours.ForEach(Debug.Log);
                foreach (PathNode neighbourNode in neighbours)
                {
                    if (exploredNodes.Contains(neighbourNode)) continue;

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.path = new List<PathNode>(currentNode.path);
                        neighbourNode.path.Add(neighbourNode);
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

                        openQueue.Enqueue(neighbourNode, neighbourNode.fCost);
                    }
                }
                Profiler.EndSample();
            }
            Debug.Log("OpenQueue!");
            Debug.Log("ExploredBoys!");
            //exploredNodes.ForEach(Debug.Log);
            Debug.Log("Didn't find shit lol");
            return null;
        }
        private void ResetNode(PathNode node)
        {
            node.path = new List<PathNode>();
            node.gCost = 99999999;

        }
        private void ResetNodes(HashSet<PathNode> visited)
        {
            Profiler.BeginSample("Pathfinding: Resetting PathNodes");
            foreach (PathNode node in visited)
            {
                node.path = new List<PathNode>();
                node.gCost = 99999999;
            }
            Profiler.EndSample();
        }
        private void ResetNodes(List<KeyValuePair<PathNode, float>> path)
        {
            Profiler.BeginSample("Pathfinding: Resetting PathNodes");
            foreach (KeyValuePair<PathNode, float> node in path)
            {
                node.Key.path = new List<PathNode>();
                node.Key.gCost = 99999999;
            }
            Profiler.EndSample();
        }
        private void ResetNodes(List<PathNode> path)
        {
            Profiler.BeginSample("Pathfinding: Resetting PathNodes");
            foreach (PathNode node in path)
            {
                node.path = new List<PathNode>();
                node.gCost = 99999999;
            }
            Profiler.EndSample();
        }
        public List<PathNode> FindPathInto2(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        {
            Profiler.BeginSample("Pathfinding: others");
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }
            if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
            {
                return null;
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;
            PathNode endNode = grid.GetGridObject(endX, endY).pn;

            counterPathCalls++;
            Profiler.EndSample();

            if (cache.IsCached(startNode, endNode))
            {
                Profiler.BeginSample("Pathfinding: IsCached hit at start");
                counterCacheHit++;
                List<PathNode> cached = cache.Get(startNode, endNode);
                Profiler.EndSample();
                //Debug.Log($"Found cache, startNode to endNode: {startNode} to {endNode}");
                return cached;
            }
            Profiler.BeginSample("Pathfinding: others");
            startNode.path = new List<PathNode> { startNode };
            startNode.gCost = 0;
            Profiler.EndSample();
            Profiler.BeginSample("Pathfinding: CalculateDistanceCost(startNode, endNode);");
            startNode.hCost = CalculateDistanceCost(startNode, endNode);
            Profiler.EndSample();

            Profiler.BeginSample("Pathfinding: others");
            openQueue = new PriorityQueue<PathNode>(startNode, startNode.fCost);
            HashSet<PathNode> visited = new HashSet<PathNode>();
            Profiler.EndSample();

            while (openQueue.Count > 0)
            {
                Profiler.BeginSample("Pathfinding: openQueue.Dequeue()");
                PathNode currentNode = openQueue.Dequeue();
                Profiler.EndSample();
                if (cache.IsCached(currentNode, endNode))
                {
                    Profiler.BeginSample("Pathfinding: IsCached hit merged");
                    counterCacheHit++;
                    List<PathNode> mergedPath = MergePathWithCache(startNode, endNode, currentNode.path);
                    cache.CacheEntirePath(mergedPath);
                    //Debug.Log("Found cache: MergePathWithCache");
                    //mergedPath.ForEach(Debug.Log);
                    Profiler.EndSample();
                    //ResetNodes(openList);
                    //ResetNodes(closedList);
                    Profiler.BeginSample("Pathfinding: Resetting PathNodes");
                    ResetNode(currentNode);
                    ResetNodes(openQueue.GetUnderlyingList());
                    ResetNodes(visited);
                    Profiler.EndSample();
                    return mergedPath;
                }
                foreach (PathNode adj in GetAdjacentNodes(endNode.x, endNode.y))
                {
                    if (adj == currentNode)
                    {
                        Profiler.BeginSample("Pathfinding: IsCached miss");
                        counterCacheMiss++;
                        //Debug.Log("Found adjacent: CacheEntirePath");
                        List<PathNode> finalPath = currentNode.path;
                        finalPath.Add(endNode);
                        //finalPath.ForEach(Debug.Log);
                        cache.CacheEntirePath(finalPath);
                        Profiler.EndSample();
                        Profiler.BeginSample("Pathfinding: Resetting PathNodes");
                        ResetNode(currentNode);
                        ResetNodes(openQueue.GetUnderlyingList());
                        ResetNodes(visited);
                        Profiler.EndSample();
                        return finalPath;
                    }
                }

                Profiler.BeginSample("Pathfinding: others");
                visited.Add(currentNode);
                Profiler.EndSample();

                Profiler.BeginSample("Pathfinding: foreach neighbours loop");
                List<PathNode> neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

                foreach (PathNode neighbourNode in neighbours)
                {
                    Profiler.BeginSample("Pathfinding: HashSet contains?");
                    bool prevvisited = visited.Contains(neighbourNode);
                    Profiler.EndSample();
                    if (prevvisited) continue;

                    Profiler.BeginSample("Pathfinding: others");
                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    Profiler.EndSample();
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        Profiler.BeginSample("Pathfinding: int tentativeGCost =");
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                        Profiler.EndSample();
                        Profiler.BeginSample("!neighbourNode.path.Any()");
                        if (!neighbourNode.path.Any())
                        {
                            openQueue.Enqueue(neighbourNode, neighbourNode.fCost);
                        }
                        Profiler.EndSample();
                        Profiler.BeginSample("Pathfinding: copy neighbourNode path");
                        neighbourNode.path = new List<PathNode>(currentNode.path) { neighbourNode };
                        Profiler.EndSample();
                    }
                }
                Profiler.EndSample();
            }
            Profiler.BeginSample("Pathfinding: failed to find path!");
            Profiler.EndSample();
            return null;
        }
        //public List<PathNode> FindPathInto2(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        //{
        //    if (pfconfig == null)
        //    {
        //        pfconfig = new PathfindingConfig();
        //    }
        //    if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
        //    {
        //        return null;
        //    }


        //    PathNode startNode = grid.GetGridObject(startX, startY).pn;
        //    PathNode endNode = grid.GetGridObject(endX, endY).pn;

        //    counterPathCalls++;

        //    if (cache.IsCached(startNode, endNode))
        //    {
        //        Profiler.BeginSample("Pathfinding: IsCached hit");
        //        counterCacheHit++;
        //        List<PathNode> cached = cache.Get(startNode, endNode);
        //        Profiler.EndSample();
        //        //Debug.Log($"Found cache, startNode to endNode: {startNode} to {endNode}");
        //        return cached;
        //    }

        //    openList = new List<PathNode> { startNode };
        //    closedList = new List<PathNode>();

        //    //for (int x = 0; x < grid.GetWidth(); x++)
        //    //{
        //    //    for (int y = 0; y < grid.GetHeight(); y++)
        //    //    {
        //    //        Profiler.BeginSample("Pathfinding: GetGridObject");
        //    //        PathNode pathNode = grid.GetGridObject(x, y).pn;
        //    //        Profiler.EndSample();
        //    //        Profiler.BeginSample("Pathfinding: new List<PathNode> { pathNode };");
        //    //        pathNode.path = new List<PathNode> { pathNode };
        //    //        Profiler.EndSample();
        //    //        Profiler.BeginSample("Pathfinding: gCost = 9999999");
        //    //        pathNode.gCost = 99999999;
        //    //        Profiler.EndSample();
        //    //    }
        //    //}
        //    startNode.path = new List<PathNode>();
        //    startNode.gCost = 0;
        //    Profiler.BeginSample("Pathfinding: CalculateDistanceCost(startNode, endNode);");
        //    startNode.hCost = CalculateDistanceCost(startNode, endNode);
        //    Profiler.EndSample();

        //    while (openList.Count > 0)
        //    {
        //        Profiler.BeginSample("Pathfinding: GetLowestFCostNode(openList);");
        //        PathNode currentNode = GetLowestFCostNode(openList);
        //        Profiler.EndSample();
        //        if (cache.IsCached(currentNode, endNode))
        //        {
        //            Profiler.BeginSample("Pathfinding: IsCached hit");
        //            counterCacheHit++;
        //            List<PathNode> mergedPath = MergePathWithCache(startNode, endNode, currentNode.path);
        //            cache.CacheEntirePath(mergedPath);
        //            //Debug.Log("Found cache: MergePathWithCache");
        //            //mergedPath.ForEach(Debug.Log);
        //            Profiler.EndSample();
        //            //ResetNodes(openList);
        //            //ResetNodes(closedList);
        //            Profiler.BeginSample("Pathfinding: Resetting PathNodes");
        //            ResetNodes(openList);
        //            ResetNodes(closedList);
        //            Profiler.EndSample();
        //            return mergedPath;
        //        }
        //        //if (currentNode == endNode)
        //        //{
        //        //    Profiler.BeginSample("Pathfinding: IsCached miss");
        //        //    counterCacheMiss++;
        //        //    //Debug.Log("Found current: CacheEntirePath");
        //        //    cache.CacheEntirePath(currentNode.path);
        //        //    Profiler.EndSample();
        //        //    ResetNodes(openList);
        //        //    ResetNodes(closedList);
        //        //    return currentNode.path;
        //        //}
        //        foreach (PathNode adj in GetAdjacentNodes(endNode.x, endNode.y))
        //        {
        //            if (adj == currentNode)
        //            {
        //                Profiler.BeginSample("Pathfinding: IsCached miss");
        //                counterCacheMiss++;
        //                //Debug.Log("Found adjacent: CacheEntirePath");
        //                List<PathNode> finalPath = currentNode.path;
        //                finalPath.Add(endNode);
        //                //finalPath.ForEach(Debug.Log);
        //                cache.CacheEntirePath(finalPath);
        //                Profiler.EndSample();
        //                Profiler.BeginSample("Pathfinding: Resetting PathNodes");
        //                ResetNodes(openList);
        //                ResetNodes(closedList);
        //                Profiler.EndSample();
        //                return finalPath;
        //            }
        //        }

        //        openList.Remove(currentNode);
        //        closedList.Add(currentNode);

        //        Profiler.BeginSample("Pathfinding: foreach neighbours loop");
        //        List<PathNode> neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

        //        foreach (PathNode neighbourNode in neighbours)
        //        {
        //            if (closedList.Contains(neighbourNode)) continue;



        //            int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
        //            if (tentativeGCost < neighbourNode.gCost)
        //            {
        //                neighbourNode.path = new List<PathNode>(currentNode.path);
        //                neighbourNode.path.Add(neighbourNode);
        //                neighbourNode.gCost = tentativeGCost;
        //                neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

        //                if (!openList.Contains(neighbourNode))
        //                {
        //                    openList.Add(neighbourNode);
        //                }
        //            }
        //        }
        //        Profiler.EndSample();
        //    }
        //    //Debug.Log("Found no path");
        //    // Out of nodes on the openList
        //    return new List<PathNode>();
        //}
        //public List<PathNode> FindPathInto2(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        //{
        //    if (pfconfig == null)
        //    {
        //        pfconfig = new PathfindingConfig();
        //    }
        //    if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
        //    {
        //        return null;
        //    }


        //    PathNode startNode = grid.GetGridObject(startX, startY).pn;
        //    PathNode endNode = grid.GetGridObject(endX, endY).pn;

        //    counterPathCalls++;

        //    if (cache.IsCached(startNode, endNode))
        //    {
        //        Profiler.BeginSample("Pathfinding: IsCached hit");
        //        counterCacheHit++;
        //        List<PathNode> cached = cache.Get(startNode, endNode);
        //        Profiler.EndSample();
        //        //Debug.Log($"Found cache, startNode to endNode: {startNode} to {endNode}");
        //        return cached;
        //    }

        //    openList = new List<PathNode> { startNode };
        //    closedList = new List<PathNode>();

        //    //for (int x = 0; x < grid.GetWidth(); x++)
        //    //{
        //    //    for (int y = 0; y < grid.GetHeight(); y++)
        //    //    {
        //    //        Profiler.BeginSample("Pathfinding: GetGridObject");
        //    //        PathNode pathNode = grid.GetGridObject(x, y).pn;
        //    //        Profiler.EndSample();
        //    //        Profiler.BeginSample("Pathfinding: new List<PathNode> { pathNode };");
        //    //        pathNode.path = new List<PathNode> { pathNode };
        //    //        Profiler.EndSample();
        //    //        Profiler.BeginSample("Pathfinding: gCost = 9999999");
        //    //        pathNode.gCost = 99999999;
        //    //        Profiler.EndSample();
        //    //    }
        //    //}
        //    startNode.path = new List<PathNode>();
        //    startNode.gCost = 0;
        //    Profiler.BeginSample("Pathfinding: CalculateDistanceCost(startNode, endNode);");
        //    startNode.hCost = CalculateDistanceCost(startNode, endNode);
        //    Profiler.EndSample();

        //    while (openList.Count > 0)
        //    {
        //        Profiler.BeginSample("Pathfinding: GetLowestFCostNode(openList);");
        //        PathNode currentNode = GetLowestFCostNode(openList);
        //        Profiler.EndSample();
        //        if (cache.IsCached(currentNode, endNode))
        //        {
        //            Profiler.BeginSample("Pathfinding: IsCached hit");
        //            counterCacheHit++;
        //            List<PathNode> mergedPath = MergePathWithCache(startNode, endNode, currentNode.path);
        //            cache.CacheEntirePath(mergedPath);
        //            //Debug.Log("Found cache: MergePathWithCache");
        //            //mergedPath.ForEach(Debug.Log);
        //            Profiler.EndSample();
        //            //ResetNodes(openList);
        //            //ResetNodes(closedList);
        //            Profiler.BeginSample("Pathfinding: Resetting PathNodes");
        //            ResetNodes(openList);
        //            ResetNodes(closedList);
        //            Profiler.EndSample();
        //            return mergedPath;
        //        }
        //        //if (currentNode == endNode)
        //        //{
        //        //    Profiler.BeginSample("Pathfinding: IsCached miss");
        //        //    counterCacheMiss++;
        //        //    //Debug.Log("Found current: CacheEntirePath");
        //        //    cache.CacheEntirePath(currentNode.path);
        //        //    Profiler.EndSample();
        //        //    ResetNodes(openList);
        //        //    ResetNodes(closedList);
        //        //    return currentNode.path;
        //        //}
        //        foreach (PathNode adj in GetAdjacentNodes(endNode.x, endNode.y))
        //        {
        //            if (adj == currentNode)
        //            {
        //                Profiler.BeginSample("Pathfinding: IsCached miss");
        //                counterCacheMiss++;
        //                //Debug.Log("Found adjacent: CacheEntirePath");
        //                List<PathNode> finalPath = currentNode.path;
        //                finalPath.Add(endNode);
        //                //finalPath.ForEach(Debug.Log);
        //                cache.CacheEntirePath(finalPath);
        //                Profiler.EndSample();
        //                Profiler.BeginSample("Pathfinding: Resetting PathNodes");
        //                ResetNodes(openList);
        //                ResetNodes(closedList);
        //                Profiler.EndSample();
        //                return finalPath;
        //            }
        //        }

        //        openList.Remove(currentNode);
        //        closedList.Add(currentNode);

        //        Profiler.BeginSample("Pathfinding: foreach neighbours loop");
        //        List<PathNode> neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

        //        foreach (PathNode neighbourNode in neighbours)
        //        {
        //            if (closedList.Contains(neighbourNode)) continue;



        //            int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
        //            if (tentativeGCost < neighbourNode.gCost)
        //            {
        //                neighbourNode.path = new List<PathNode>(currentNode.path);
        //                neighbourNode.path.Add(neighbourNode);
        //                neighbourNode.gCost = tentativeGCost;
        //                neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

        //                if (!openList.Contains(neighbourNode))
        //                {
        //                    openList.Add(neighbourNode);
        //                }
        //            }
        //        }
        //        Profiler.EndSample();
        //    }
        //    //Debug.Log("Found no path");
        //    // Out of nodes on the openList
        //    return new List<PathNode>();
        //}

        public List<PathNode> FindPathInto(int startX, int startY, int endX, int endY, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }
            if (grid.GetGridObject(startX, startY) == null || grid.GetGridObject(endX, endY) == null)
            {
                new List<PathNode>();
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;
            PathNode endNode = grid.GetGridObject(endX, endY).pn;

            if (cache.IsCached(startNode, endNode)) return cache.Get(startNode, endNode);

            openList = new List<PathNode> { startNode };
            closedList = new List<PathNode>();

            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    PathNode pathNode = grid.GetGridObject(x, y).pn;
                    pathNode.gCost = 99999999;
                    pathNode.cameFromNode = null;
                }
            }

            startNode.gCost = 0;
            startNode.hCost = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);

                //if (cache.IsCached(currentNode, endNode)) return MergePathWithCache(goal, startNode, parentMap, current);
                
                if (currentNode == endNode)
                {
                    return CalculatePath(currentNode);
                }
                else
                {
                    foreach (PathNode adj in GetAdjacentNodes(endNode.x, endNode.y))
                    {
                        if (adj == currentNode)
                        {
                            List<PathNode> final = CalculatePath(adj);
                            final.Add(endNode);
                            return final;
                        }
                    }
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                List<PathNode> neighbours;


                neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

                foreach (PathNode neighbourNode in neighbours)
                {
                    if (closedList.Contains(neighbourNode)) continue;

                    int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;
                        neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }
            // Out of nodes on the openList
            return new List<PathNode>(); ;
        }
        public List<PathNode> FindPathNodesWithinRange(int startX, int startY, int range, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }
            List<PathNode> openList = new List<PathNode> { GetNode(startX, startY) };
            List<PathNode> removeList = new List<PathNode>();
            List<PathNode> addList = new List<PathNode>();
            List<PathNode> finalList = new List<PathNode>();
            List<PathNode> closedList = new List<PathNode>();
            int counter = 0;

            while (counter < range)
            {
                foreach (PathNode node in openList)
                {
                    closedList.Add(node);
                    removeList.Add(node);
                    List<PathNode> neighbours;

                    neighbours = GetNeighbourListFiltered(node, pfconfig);

                    foreach (PathNode neighbour in neighbours)
                    {
                        if (closedList.Contains(neighbour)) continue;
                        if (openList.Contains(neighbour)) continue;
                        if (addList.Contains(neighbour)) continue;

                        finalList.Add(neighbour);
                        addList.Add(neighbour);
                    }
                }
                foreach (PathNode node in removeList)
                {
                    openList.Remove(node);
                }
                foreach (PathNode node in addList)
                {
                    openList.Add(node);
                }
                removeList = new List<PathNode>();
                addList = new List<PathNode>();
                counter++;
            }
            return finalList;
        }

        public PathNode GetNode(int x, int y)
        {
            return grid.GetGridObject(x, y).pn;
        }
        public List<PathNode> GetAdjacentNodes(int x, int y)
        {

            return new List<GridContainer> { grid.GetGridObject(x-1, y),
            grid.GetGridObject(x+1, y),
            grid.GetGridObject(x, y-1),
            grid.GetGridObject(x, y+1)
        }.Where(gc => gc != null)
            .Select(gc => gc.pn)
            .ToList();
        }
        public bool DoesPathNodeHavePlayerActor(PathNode p)
        {
            if (p.parent.actor == null)
            {
                return false;
            }
            if (p.parent.actor.GetComponent<Faction>() == null)
            {
                return false;
            }
            if (p.parent.actor.GetComponent<Faction>().faction != "Player")
            {
                return false;
            }
            return true;
        }
        public List<PathNode> GetPathToClosestActor(int fromX, int fromY, List<GameObject> actors)
        {
            List<PathNode> closestPath = new List<PathNode>();
            int minDistance = int.MaxValue;
            foreach (GameObject actor in actors)
            {
                Position actorPos = actor.GetComponent<Position>();
                if (actorPos == null)
                {
                    throw ProgramUtils.MissingComponentException(typeof(Position));
                }
                List<PathNode> path = FindPath(fromX, fromY, actorPos.x, actorPos.y);
                if (path.Count == 0)
                {
                    continue;
                }
                if (path.Count - 1 < minDistance)
                {
                    closestPath = path;
                    minDistance = closestPath.Count - 1;
                }
            }
            return closestPath;
        }
        public PathNode GetClosestPlayerActorPosition(List<GameObject> actors, int x, int y)
        {
            return GetPathToClosestActor(x, y, actors).Last();
        }
        private List<PathNode> GetNeighbourListFiltered(PathNode currentNode, PathfindingConfig pfconfig)
        {
            List<PathNode> neighbourList = new List<PathNode>();

            foreach (Vector2Int dir in directions)
            {
                if (currentNode.x + dir.x >= 0 && currentNode.x + dir.x < grid.GetWidth() &&
                    currentNode.y + dir.y >= 0 && currentNode.y + dir.y < grid.GetHeight())
                {
                    PathNode neighbour = GetNode(currentNode.x + dir.x, currentNode.y + dir.y);
                    if (neighbour.CheckConditions(pfconfig))
                    {
                        neighbourList.Add(neighbour);
                    }
                }
            }

            return neighbourList;
        }
        private List<PathNode> CalculatePath(PathNode endNode)
        {
            List<PathNode> path = new List<PathNode>();
            path.Add(endNode);
            PathNode currentNode = endNode;
            while (currentNode.cameFromNode != null)
            {
                path.Add(currentNode.cameFromNode);
                currentNode = currentNode.cameFromNode;
            }
            path.Reverse();
            return path;
        }
        private const int MOVE_STRAIGHT_COST = 10;
        private const int MOVE_DIAGONAL_COST = 14;
        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int zDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - zDistance);
            return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
        }
        private PathNode GetLowestFCostNode(List<PathNode> pathNodeList)
        {
            PathNode lowestFCostNode = pathNodeList[0];
            for (int i = 1; i < pathNodeList.Count; i++)
            {
                if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                {
                    lowestFCostNode = pathNodeList[i];
                }
            }
            return lowestFCostNode;
        }
    }
}

//    neighbourNode.gCost = neighbourGCost;
                //    int neighbourHCost = CalculateDistanceCost(neighbourNode, endNode);

                //    List<PathNode> neighbourPath = new List<PathNode>(currentState.path)
                //        {
                //            neighbourNode
                //        };

                //    State neighbourState = new State(neighbourNode, neighbourPath, neighbourHCost, neighbourGCost);
                //    openQueue.Enqueue(neighbourState, neighbourGCost + neighbourHCost);

                //    if (!openQueue.Contains(neighbourState))
                //    {
                //        openQueue.Enqueue(neighbourState, neighbourGCost + neighbourHCost);
                //    }
                //    else
                //    {
                //        openQueue.Replace(neighbourNode);
                //    }
                //}

                //if (DateTime.Now.Millisecond - timer > 5000)
                //{
                //    Debug.Log("Took too long");
                //    return null;
                //}
                ////int neighbourGCost = currentState.path.Count + CalculateDistanceCost(currentState.node, neighbourNode);
                ////int neighbourHCost = CalculateDistanceCost(neighbourNode, endNode);
                //////int neighbourHCost = CalculateDistanceCost(currentState.node, neighbourNode);

                ////List<PathNode> neighbourPath = new List<PathNode>(currentState.path);
                ////neighbourPath.Add(neighbourNode);
                ////State neighbour = new State(neighbourNode, neighbourPath, neighbourHCost);
                ////openQueue.Enqueue(neighbour, neighbourGCost + neighbourHCost);

                ////if (neighbourGCost < neighbourNode.gCost)
                ////{


                ////    neighbourNode.cameFromNode = currentNode;
                ////    neighbourNode.gCost = tentativeGCost;
                ////    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                ////    neighbourNode.fCost;

                ////    if (!openList.Contains(neighbourNode))
                ////    {
                ////        openList.Add(neighbourNode);
                ////    }
                ////}
                //}