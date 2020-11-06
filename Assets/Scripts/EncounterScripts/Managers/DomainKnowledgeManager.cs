using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class DomainKnowledgeManager : MonoBehaviour
    {
        private class TileEvaluation
        {
            public PathNode position;
            public float visibility;
            public float proximityToEnemies;
            public float proximityToAllies;
            public float distanceToAttackObjective;

            public TileEvaluation(PathNode pn)
            {
                this.position = pn;

                Reset();
            }

            public void Reset()
            {
                this.visibility = 0;
                this.proximityToAllies = 0;
                this.proximityToEnemies = 0;
                this.distanceToAttackObjective = 0;
            }

            public override string ToString()
            {
                return string.Format("({0},{1},{2},{3})", visibility, proximityToEnemies, proximityToAllies, distanceToAttackObjective);
            }
        }

        private GlobalEventManager gem;
        private MapManager mgm;
        private Pathfinding pf;
        private FactionManager fm;

        private List<TileEvaluation> tileEvaluations;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
        }
        public IEnumerator GenerateTileEvaluations(int x, int y, int moveRange = 5, int attackRange = 5)
        {
            if (tileEvaluations == null)
            {
                Debug.Log($"tiles count: {mgm.grid.tiles.Count()}");
                tileEvaluations = mgm.grid.tiles.Select(gc => new TileEvaluation(gc.pn)).ToList();
                Debug.Log($"tileEvaluations count: {tileEvaluations.Count()}");
            }
            tileEvaluations = mgm.grid.GetRectangularArea(x, y, moveRange, moveRange)
                .Where(gc => gc.pn.isWalkable)
                .Select(gc => new TileEvaluation(gc.pn))
                .ToList();

            List<Task> routines = new List<Task>();
            foreach (TileEvaluation te in tileEvaluations)
            {
                routines.Add(Task.Get(CalculateDomainKnowledgeAt(x, y, te, moveRange + attackRange), true));
            }
            foreach (Task co in routines)
            {
                while (co.Running)
                {
                    yield return co;
                }
            }
        }
        private IEnumerator CalculateDomainKnowledgeAt(int currentX, int currentY, TileEvaluation te, int domainRange)
        {
            if (domainRange >= 5)
            {
                domainRange = 5;
            }
            te.Reset();

            IEnumerable<GridContainer> relevantTiles = mgm.grid
                .GetRectangularArea(te.position.x, te.position.y, domainRange, domainRange)
                .Where(gc => gc.pn.isWalkable);

            foreach (GridContainer grid in relevantTiles)
            {
                if (grid.x == currentX && grid.y == currentY)
                {
                    continue;
                }
                te.visibility = 0;

                if (fm.GetFactionOf(grid.actor) == "Enemy")
                {
                    float dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(te.position.x - grid.x), 2) + Mathf.Pow(Mathf.Abs(te.position.y - grid.y), 2));
                    if (dist != 0)
                    {
                        te.proximityToAllies += Constants.ActorProximityFunc(dist);
                    }
                }
                if (fm.GetFactionOf(grid.actor) == "Player")
                {

                    float dist = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(te.position.x - grid.x), 2) + Mathf.Pow(Mathf.Abs(te.position.y - grid.y), 2));
                    if (dist != 0)
                    {
                        te.proximityToEnemies += Constants.ActorProximityFunc(dist);
                    }
                }
            }

            IEnumerable<GameObject> kings = fm.GetFaction("Player")
                .Where(actor => actor.name.Contains("King") && actor.transform.position.x != 0);
            if (kings.Count() != 0)
            {
                Position kingPos = kings.First().GetComponent<Position>();

                PathEnumerator pathGen = pf.AsyncFindPath(te.position.x, te.position.y, kingPos.x, kingPos.y, new PathfindingConfig(false, true, true));
                yield return pathGen.Coroutine;
                List<PathNode> path = pathGen.Result;

                if (path == null)
                {
                    te.distanceToAttackObjective -= pf.ManhattanDistance(te.position.x, te.position.y, kingPos.x, kingPos.y);
                }
                else
                {
                    float objdist = path.Sum(p => p.walkIntoCost);

                    te.distanceToAttackObjective -= objdist;
                }
            }
        }
        public float EvaluatePositionalStrength(PathNode pn, float visibilityMultiplier, float proximityToEnemiesMultiplier, float proximityToAlliesMultiplier, float distanceToObjectiveMultiplier)
        {
            TileEvaluation te = tileEvaluations.Find(tev => tev.position.x == pn.x && tev.position.y == pn.y);

            //ProgramUtils.PrintIEnumerable(tileEvaluations);

            if (te == null)
            {
                throw new Exception("Invalid grid coordinates");
                // 21, 13 finns inte,
                // dock fanns 21, 11 och 21, 15
            }

            float accumulativeStrength = visibilityMultiplier * te.visibility +
                                         proximityToEnemiesMultiplier * te.proximityToEnemies +
                                         proximityToAlliesMultiplier * te.proximityToAllies +
                                         distanceToObjectiveMultiplier * te.distanceToAttackObjective;

            return accumulativeStrength;
        }
    }
}