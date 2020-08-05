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
        private PathNode objective;

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
        void Start()
        {
            tileEvaluations = mgm.grid.tiles.Select(gc => new TileEvaluation(gc.pn)).ToList();
        }
        public void GenerateTileEvaluations(int x, int y)
        {
            Profiler.BeginSample("GenerateTileEvaluations: mgm.grid.tiles Where Select");
            // Code to measure...
            //Debug.Log($"x:{x}, y:{y}");
            tileEvaluations = mgm.grid.GetRectangularArea(x, y, Constants.NEARBY_RADIUS, Constants.NEARBY_RADIUS)
                .Where(gc => gc.pn.isWalkable && !gc.pn.hasActor)
                .Select(gc => new TileEvaluation(gc.pn))
                .ToList();
            //Debug.Log("GenerateTileEvaluations: tileEvaluations.Count");
            //Debug.Log(tileEvaluations.Count);

            //tileEvaluations.Select(te => te.position).ToList().ForEach(Debug.Log);

            //tileEvaluations = mgm.grid.tiles
            //    .Where(gc => Mathf.Abs(gc.x - x) < Constants.NEARBY_RADIUS && Mathf.Abs(gc.y - y) < Constants.NEARBY_RADIUS)
            //    .Select(gc => new TileEvaluation(gc.pn))
            //    .ToList();
            Profiler.EndSample();
            Profiler.BeginSample("GenerateTileEvaluations: foreach (TileEvaluation te in tileEvaluations)");
            foreach (TileEvaluation te in tileEvaluations)
            {
                CalculateDomainKnowledgeAt(te);
            }
            Profiler.EndSample();
        }
        private int calculateTileCounter = 0;
        private void CalculateDomainKnowledgeAt(TileEvaluation te)
        {
            Profiler.BeginSample("CalculateDomainKnowledgeAt: te.Reset()");
            te.Reset();
            Profiler.EndSample();

            Profiler.BeginSample("CalculateDomainKnowledgeAt: relevantTiles Where Linq");
            IEnumerable<GridContainer> relevantTiles = mgm.grid
                .GetRectangularArea(te.position.x, te.position.y, Constants.DOMAIN_KNOWLEDGE_RADIUS, Constants.DOMAIN_KNOWLEDGE_RADIUS)
                .Where(gc => gc.pn.isWalkable && !gc.pn.hasActor);

            //Debug.Log("CalculateDomainKnowledgeAt: relevantTiles.Count");
            //relevantTiles.Select(gc => gc.pn).ToList().ForEach(Debug.Log);
            Profiler.EndSample();
            Profiler.BeginSample("CalculateDomainKnowledgeAt: foreach (GridContainer grid in relevantTiles)");
            foreach (GridContainer grid in relevantTiles)//mgm.grid.tiles)
            {
                calculateTileCounter++;
                te.visibility = 0;
                Profiler.BeginSample("CalculateDomainKnowledgeAt: foreach: fm.GetFactionOf(grid.actor) == \"Pirate\"");
                if (fm.GetFactionOf(grid.actor) == "Pirate")
                {
                    //float dist = pf.FindPathInto2(te.position.x, te.position.y, grid.x, grid.y).Count - 1;
                    float dist = Mathf.Abs(te.position.x - grid.x) + Mathf.Abs(te.position.y - grid.y);// pf.FindPathInto2(te.position.x, te.position.y, grid.x, grid.y).Count - 1;
                    if (dist != 0)
                    {
                        te.proximityToAllies += 1 / dist;
                    }
                }
                Profiler.EndSample();
                Profiler.BeginSample("CalculateDomainKnowledgeAt: foreach: fm.GetFactionOf(grid.actor) == \"Player\"");
                if (fm.GetFactionOf(grid.actor) == "Player")
                {
                    //float dist = pf.FindPathInto2(te.position.x, te.position.y, grid.x, grid.y).Count - 1;
                    float dist = Mathf.Abs(te.position.x - grid.x) + Mathf.Abs(te.position.y - grid.y);
                    if (dist != 0)
                    {
                        te.proximityToEnemies += 1 / dist;
                    }
                }
                Profiler.EndSample();
            }
            Profiler.EndSample();

            Profiler.BeginSample("CalculateDomainKnowledgeAt: kings Where Linq");
            IEnumerable<GameObject> kings = fm.GetFaction("Player")
                .Where(actor => actor.name.Contains("King") && actor.transform.position.x != 0);
            Profiler.EndSample();
            if (kings.Count() != 0)
            {
                Position kingPos = kings.First().GetComponent<Position>();
                //Debug.Log($"Position of King: {kingPos}");
                Profiler.BeginSample("CalculateDomainKnowledgeAt: kings FindPathInto");
                float objdist = pf.FindPathInto2(te.position.x, te.position.y, kingPos.x, kingPos.y).Count - 1;
                //Debug.Log($"objdist: {objdist}");
                Profiler.EndSample();
                te.distanceToAttackObjective += -objdist;
            }
        }
        public float EvaluatePositionalStrength(PathNode pn, float visibilityMultiplier, float proximityToEnemiesMultiplier, float proximityToAlliesMultiplier, float distanceToObjectiveMultiplier)
        {
            //Debug.Log("EvaluatePositionalStrength: tileEvaluations.Count");
            TileEvaluation te = tileEvaluations.Find(tev => tev.position.x == pn.x && tev.position.y == pn.y);

            //tileEvaluations.ForEach(Debug.Log);

            //Debug.Log(pn);

            if (te == null)
            {
                throw new Exception("Invalid grid coordinates");
            }

            float accumulativeStrength = visibilityMultiplier * te.visibility +
                                         proximityToEnemiesMultiplier * te.proximityToEnemies +
                                         proximityToAlliesMultiplier * te.proximityToAllies +
                                         distanceToObjectiveMultiplier * te.distanceToAttackObjective;

            return accumulativeStrength;
        }
    }
}