using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class PirateAIManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private DomainKnowledgeManager dkm;
        private Pathfinding pf;
        private FactionManager fm;

        private List<GameObject> pirates;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (dkm = FindObjectOfType(typeof(DomainKnowledgeManager)) as DomainKnowledgeManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }

            gem.StartListening("BeginAITurn", StartTurn);
        }
        void OnDestroy()
        {
            gem.StopListening("BeginAITurn", StartTurn);
        }

        private void StartTurn(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            pirates = fm.GetFaction("Pirate");
            RemoveDestroyedPirates();
            StartCoroutine(StartTurn());
        }
        private IEnumerator StartTurn()
        {
            Profiler.BeginSample("StartTurn: SortPiratesByProximityToPlayerActor();");
            SortPiratesByProximityToPlayerActor();
            Profiler.EndSample();
            foreach (GameObject pirate in pirates)
            {
                Profiler.BeginSample("StartTurn: foreach");
                Position pos = pirate.GetComponent<Position>();
                if (pos == null)
                {
                    throw ProgramUtils.MissingComponentException(typeof(Position));
                }
                Profiler.BeginSample("StartTurn: dkm.GenerateTileEvaluations(pos.x, pos.y);");
                dkm.GenerateTileEvaluations(pos.x, pos.y);
                Profiler.EndSample();

                Profiler.BeginSample("StartTurn: BehaviourHandler bh = pirate.GetComponent<BehaviourHandler>();");
                BehaviourHandler bh = pirate.GetComponent<BehaviourHandler>();
                Profiler.EndSample();

                Profiler.BeginSample("StartTurn: List<Analysis> analyses = bh.GetAnalyses();");
                List<Analysis> analyses = bh.GetAnalyses();
                Profiler.EndSample();

                Profiler.BeginSample("StartTurn: List<Decision> bestCourseOfAction = Evaluate(analyses);");
                List<Decision> bestCourseOfAction = Evaluate(analyses);
                Profiler.EndSample();
                Profiler.EndSample();

                yield return StartCoroutine(bh.Execute(bestCourseOfAction));
            }

            gem.TriggerEvent("EndAITurn");
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
            Position kingPos = fm.GetFaction("Player")
                .Where(actor => actor.name.Contains("King") && actor.transform.position.x != 0)
                .First()
                .GetComponent<Position>();
            pirates.Sort((p1, p2) =>
            {
                Position pos1 = p1.GetComponent<Position>();
                Position pos2 = p2.GetComponent<Position>();
                int distance1 = Mathf.Abs(pos1.x - kingPos.x) + Mathf.Abs(pos1.y - kingPos.y);
                int distance2 = Mathf.Abs(pos2.x - kingPos.x) + Mathf.Abs(pos2.y - kingPos.y);

                return distance1.CompareTo(distance2);
            });
        }
    }
}