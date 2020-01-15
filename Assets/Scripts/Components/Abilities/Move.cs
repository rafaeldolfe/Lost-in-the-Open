using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(Position))]
[RequireComponent(typeof(AbilitiesHandler))]
public class Move : Ability
{
    private GlobalEventManager gem;
    private AbilitiesHandler ah;
    private Position pos;

    private bool ready = true;
    private Vector3 target;
    private int usedMoves;

    public float moveSpeed;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(ah = gameObject.GetComponent<AbilitiesHandler>());
        deps.Add(pos = gameObject.GetComponent<Position>());
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        usedMoves = 0;
        category = "Movement";
        highlightColor = new Color(0.22f, 0.7f, 0.22f, 0.78f); // Green
        pfconfig = new PathfindingConfig(ignoresTerrain: false, ignoresActors: false);
        gem.StartListening("Move", MoveHandler);
    }
    void OnDestroy()
    {
        gem.StopListening("Move", MoveHandler);
    }
    void Update()
    {
        if (!ready)    
        {
            return;
        }   
        if (q.Count == 0) {
            return;
        }

        target = ConsumeMove();

        gem.TriggerEvent("Move", gameObject, x: pos.x, z: pos.z, tx: (int)Math.Round(target.x), tz: (int)Math.Round(target.z));
    }

    public void MoveHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (invoker != gameObject)
        {
            return;
        }
        int traversal = Math.Abs(x - tx) + Math.Abs(z - tz);
        if (traversal > 1)
        {
            throw new Exception("Expected movement to be 1 tile straight, found " + traversal + " moves along x or z");
        }

        usedMoves += traversal;

        pos.MoveTo(tx, tz);

        ready = false;
        
        StartCoroutine(MoveOverSpeed(gameObject, target, moveSpeed));
    }
    public override int GetRange()
    {
        if (range < usedMoves)
        {
            throw new Exception("Expected Move range to be bigger than or equal to the amount of used moves");
        }
        return range - usedMoves;
    }
    public override void UseAbility(List<PathNode> path)
    {
        if (Done())
        {
            throw new Exception("Tried to move without any remaining moves, or when the actor was not ready");
        }
        if (Status() == "Busy")
        {
            throw new Exception("Tried to move while actor was busy");
        }
        
        AddMoves(path.Skip(1).ToList());
    }
    public override bool Done()
    {
        return usedMoves == range && ready;
    }
    public override string Status()
    {
        return q.Count == 0 && ready ? "Idle" : "Busy";
    }
    public override void Reset(List<object> parameters)
    {
        usedMoves = 0;
    }
    private IEnumerator routine;
    public IEnumerator currentMovement;
    public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
    {
        // speed should be 1 unit per second
        while (objectToMove.transform.position != end)
        {
            objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        ready = true;

        if (Done())
        {
            ah.AbilityDone();
        }
    }
    public IEnumerator MoveOverSeconds(GameObject objectToMove, Vector3 end, float seconds)
    {
        float elapsedTime = 0;
        Vector3 startingPos = objectToMove.transform.position;
        while (elapsedTime < seconds)
        {
            objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
            elapsedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        objectToMove.transform.position = end;
        ready = true;

        if (Done())
        {
            ah.AbilityDone();
        }
    }
    public Queue<Vector3> q = new Queue<Vector3>();
    public void AddMoves(List<PathNode> path)
    {
        foreach (PathNode p in path)
        {
            q.Enqueue(new Vector3(p.x, p.parent.height, p.z));
        }
    }
    public Vector3 ConsumeMove()
    {
        return q.Dequeue();
    }
}
