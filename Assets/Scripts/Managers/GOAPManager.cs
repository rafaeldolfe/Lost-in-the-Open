using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System;

public class GOAPManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private Pathfinding pf;
    private FactionManager fm;

    private List<GameObject> pirates;
    private List<Goal> goals;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
        };
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }
    void Start()
    {
        gem.StartListening("BeginAITurn", StartTurn);

        pirates = fm.GetFaction("Pirate");
    }
    void OnDestroy()
    {
        gem.StopListening("BeginAITurn", StartTurn);
    }

    private void StartTurn(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        RemoveDestroyedPirates();
        StartCoroutine(StartTurn());
    }


    private IEnumerator StartTurn()
    {
        SortPiratesByProximityToPlayerActor();
        foreach (GameObject pirate in pirates)
        {
            BehaviourHandler bh = pirate.GetComponent<BehaviourHandler>();
            if (bh == null)
            {
                throw new Exception("Missing component: Could not find BehaviourHandler on " + pirate.name);
            }

            List<Analysis> analyses = bh.GetAnalyses();

            List<Decision> bestCourseOfAction = Evaluate(analyses);

            yield return StartCoroutine(bh.Execute(bestCourseOfAction));
        }

        gem.TriggerEvent("EndAITurn", gameObject);
    }
    private List<Decision> Evaluate(List<Analysis> analyses)
    {
        List<Decision> bestCourseOfAction = new List<Decision>();
        if (analyses[0].coursesOfAction.Count == 0)
        {
            return bestCourseOfAction;
        }
        bestCourseOfAction = analyses[0].coursesOfAction[0];
        return bestCourseOfAction;
    }
    private void RemoveDestroyedPirates()
    {
        pirates = pirates.Where(p => p != null).ToList();
    }
    private void SortPiratesByProximityToPlayerActor()
    {
        List<GameObject> playerActors = fm.GetFaction("Player");
        pirates.Sort((p1, p2) =>
        {
            Position pos1 = p1.GetComponent<Position>();
            Position pos2 = p2.GetComponent<Position>();
            int distance1 = pf.GetPathToClosestActor(pos1.x, pos1.z, playerActors).Count - 1;
            int distance2 = pf.GetPathToClosestActor(pos2.x, pos2.z, playerActors).Count - 1;

            return distance1.CompareTo(distance2);
        });
    }
}

public class Goal
{
    public string name;
    public string requires;

    public Goal(string name, string requires)
    {
        this.name = name;
        this.requires = requires;
    }
}
