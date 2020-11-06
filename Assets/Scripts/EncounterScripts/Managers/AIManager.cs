using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class AIManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private DomainKnowledgeManager dkm;
        private EventLoopManager elm;
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
            (elm = FindObjectOfType(typeof(EventLoopManager)) as EventLoopManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }

            gem.StartListening("EnemyBeginTurn", StartTurn);
        }
        void OnDestroy()
        {
            gem.StopListening("EnemyBeginTurn", StartTurn);
        }

        private void StartTurn(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            pirates = fm.GetFaction("Enemy");
            RemoveDestroyedPirates();
            StartCoroutine(StartTurn());
        }
        private IEnumerator ControlActor(GameObject pirate)
        {
            pf.ResetCaches();

            Position pos = pirate.GetComponent<Position>();
            if (pos == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Position));
            }
            BehaviourHandler bh = pirate.GetComponent<BehaviourHandler>();

            yield return StartCoroutine(dkm.GenerateTileEvaluations(pos.x, pos.y, bh.GetMoveRange(), bh.GetAttackRange()));

            List<Analysis> analyses = bh.GetAnalyses();

            List<Decision> bestCourseOfAction = Evaluate(analyses);

            foreach (Decision dec in bestCourseOfAction)
            {
                if (dec.ability.GetType() is IMovement)
                {
                    GlobalDebugManager.Instance.HighlightTiles(dec.path);
                }
                List<Decision> subDecs = dec.ability.BreakDownAbility(dec.path);
                foreach (Decision partDec in subDecs)
                {
                    elm.AddEvent(partDec);
                }
            }
        }
        private IEnumerator StartTurn()
        {
            SortPiratesByProximityToPlayerActor();
            foreach (GameObject pirate in pirates)
            {
                yield return ControlActor(pirate);

                while(!elm.EventsFinished() || (elm.EventsFinished() && elm.WasInterrupted()))
                {
                    if (elm.WasInterrupted())
                    {
                        yield return ControlActor(pirate);
                        elm.InterruptionHandled();
                    }
                    else if (!elm.EventsFinished())
                    {
                        yield return null;
                    }
                }
            }

            gem.TriggerEvent("EnemyEndTurn");
        }
        internal List<(List<Decision>, float)> _GetEvaluations(GameObject actor)
        {
            Position pos = actor.GetComponent<Position>();
            if (pos == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Position));
            }

            BehaviourHandler bh = actor.GetComponent<BehaviourHandler>();

            return bh._GetEvaluationsOfFirstBehaviour();

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