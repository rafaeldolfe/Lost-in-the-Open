using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    public class VisionManager : MonoBehaviour
    {
        private class LineOfSight
        {
            public GameObject actor;
            public List<PathNode> lineOfSight;

            public LineOfSight(GameObject actor, List<PathNode> lineOfSight)
            {
                this.actor = actor;
                this.lineOfSight = lineOfSight;
            }
        }
        private GlobalEventManager gem;
        private Pathfinding pf;

        public List<PathNode> playerVisibles = new List<PathNode>();
        public List<PathNode> enemyVisibles = new List<PathNode>();

        // To be added separate vision systems for different units, if it is needed.
        private PathfindingConfig visionPathfindingConfig = new PathfindingConfig(false, false, true);
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }

            gem.StartListening("Move", MoveActor);
            gem.StartListening("Death", RemoveActor);
            gem.StartListening("RegisterUnit", RegisterUnit);
        }
        void OnDestroy()
        {
            gem.StopListening("Move", MoveActor);
            gem.StopListening("Death", RemoveActor);
            gem.StopListening("RegisterUnit", RegisterUnit);
        }

        private void RegisterUnit(GameObject instantiatedGameObject, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected list with 1 Vector2Int, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(Vector2Int))
            {
                throw new Exception(string.Format("Expected 1st item to be Vector2Int, found ", parameters[0].GetType()));
            }
            Vision vision = instantiatedGameObject.GetComponent<Vision>();
            Faction faction = instantiatedGameObject.GetComponent<Faction>();
            if (vision == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Vision));
            }
            if (faction == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Faction));
            }
            Vector2Int position = (Vector2Int)parameters[0];
            LineOfSight los = new LineOfSight(instantiatedGameObject, pf.DijkstraWithinRange(position.x, position.y, vision.range, visionPathfindingConfig));

            if (faction.IsPlayerFaction())
            {
                foreach (PathNode pn in los.lineOfSight)
                {
                    pn.playerVisionCounter++;
                }
            }
            else
            {
                foreach (PathNode pn in los.lineOfSight)
                {
                    pn.enemyVisionCounter++;
                }
            }
        }
        private void MoveActor(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            Vision vision = invoker.GetComponent<Vision>();
            Faction faction = invoker.GetComponent<Faction>();
            if (vision == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Vision));
            }
            if (faction == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(Faction));
            }
            LineOfSight los = new LineOfSight(invoker, pf.DijkstraWithinRange(x, y, vision.range, visionPathfindingConfig));

            if (faction.IsPlayerFaction())
            {
                foreach (PathNode pn in los.lineOfSight)
                {
                    if (!playerVisibles.Contains(pn))
                    {
                        playerVisibles.Add(pn);
                    }
                }
            }
            else
            {
                foreach (PathNode pn in los.lineOfSight)
                {
                    if (!enemyVisibles.Contains(pn))
                    {
                        enemyVisibles.Add(pn);
                    }
                }
            }
        }
        private void RemoveActor(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            //if (grid.GetGridObject(x, y) == null)
            //{
            //    throw new Exception("Invalid grid coordinates");
            //}
            //if (grid.GetGridObject(x, y).actor == null)
            //{
            //    throw new Exception(string.Format("Expected actor at position ({0}, {1}), but found null", x, y));
            //}
            //grid.GetGridObject(x, y).RemoveActor();
        }
    }
}
