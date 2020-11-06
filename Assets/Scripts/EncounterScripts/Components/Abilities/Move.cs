using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    [RequireComponent(typeof(Position))]
    [RequireComponent(typeof(AbilitiesHandler))]
    public class Move : ActiveAbility, IMovement
    {
        private GlobalEventManager gem;
        private Pathfinding pf;
        private AbilitiesHandler ah;
        private Position pos;

        private bool ready = true;
        [SerializeField]
        [HideInInspector]
        private float usedMoves;

        public float moveSpeed;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (ah = gameObject.GetComponent<AbilitiesHandler>()),
            (pos = gameObject.GetComponent<Position>())
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            usedMoves = 0;
            category = "Movement";
            highlightColor = new Color(0.22f, 0.7f, 0.22f, 0.78f); // Green
            pfconfig = new PathfindingConfig(ignoreAll: false, ignoreLastTile: false, ignoreActors: false);
        }
        public void Moved(PathNode target)
        {
            int x = pos.x;
            int y = pos.y;
            int tx = target.x;
            int ty = target.y;
            float traversal = (new Vector3(x, y, 0) - new Vector3(tx, ty, 0)).magnitude;
            if (traversal > 2)
            {
                throw new Exception("Expected movement to be 1 tile straight, found " + traversal + " moves along x or y");
            }

            usedMoves += traversal;

            pos.MoveTo(tx, ty);

            ready = false;
        }
        public override float GetRange()
        {
            if (range < usedMoves)
            {
                throw new Exception("Expected Move range to be bigger than or equal to the amount of used moves");
            }
            return range - usedMoves;
        }
        public override IEnumerator UseAbility(List<PathNode> targets)
        {
            if (Done())
            {
                throw new Exception("Tried to move without any remaining moves, or when the actor was not ready");
            }
            if (Status() == "Busy")
            {
                throw new Exception("Tried to move while actor was busy");
            }

            PathNode target = targets.Last();
            ready = false;
            gem.TriggerEvent("Move", gameObject, x: pos.x, y: pos.y, tx: target.x, ty: target.y);
            Moved(target);
            yield return MoveOverSpeed(gameObject, new Vector3(target.x, target.y), moveSpeed);
            ready = true;
        }
        public override IEnumerator BreakDownAbility(int tx, int ty)
        {
            PathEnumerator pathGen = pf.AsyncFindPath(pos.x, pos.y, tx, ty, pfconfig);
            yield return pathGen.Coroutine;
            List<PathNode> path = pathGen.Result;
            List<Decision> decisions = new List<Decision>();
            for (int i = 0; i < path.Count() - 1; i++)
            {
                decisions.Add(MoveBetween(path[i], path[i + 1]));
            }
            yield return decisions;
        }
        private Decision MoveBetween(PathNode p1, PathNode p2)
        {
            return new Decision(this, new List<PathNode> { p1, p2 });
        }
        public override bool Done()
        {
            if (GetTilesWithinRange().Count == 0)
            {
                return ready;
            }
            return false;
        }
        public override string Status()
        {
            return ready ? "Idle" : "Busy";
        }
        public override void Reset()
        {
            usedMoves = 0;
        }
        public override List<PathNode> GetTargetsFrom(int x, int y)
        {
            tilesWithinRange = pf.DijkstraWithinRangeCaching(this, x, y, GetRange(), pfconfig);
            tilesWithinRange.RemoveAt(0);
            return tilesWithinRange;
        }
        public override List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty)
        {
            return pf.FindPathWithinRange(this, x, y, tx, ty);
        }
        public IEnumerator currentMovement;
        public IEnumerator MoveOverSpeed(GameObject objectToMove, Vector3 end, float speed)
        {
            // speed should be 1 unit per second
            while (objectToMove.transform.position != end)
            {
                if (!PauseService.IsLevelPaused(PauseService.MENU_PAUSE))
                {
                    objectToMove.transform.position = Vector3.MoveTowards(objectToMove.transform.position, end, speed * Time.deltaTime);
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public override List<PathNode> GetTilesWithinRange()
        {
            return GetTilesWithinRange(pos.x, pos.y);
        }
        private List<PathNode> GetTilesWithinRange(int x, int y)
        {
            tilesWithinRange = pf.DijkstraWithinRange(x, y, GetRange(), pfconfig);
            tilesWithinRange.RemoveAt(0);
            return tilesWithinRange;
        }

        public override List<Decision> BreakDownAbility(List<PathNode> path)
        {
            List<Decision> decisions = new List<Decision>();
            for (int i = 0; i < path.Count() - 1; i++)
            {
                decisions.Add(MoveBetween(path[i], path[i + 1]));
            }
            return decisions;
        }

        public override List<PathNode> GetTargetTiles(int tx, int ty)
        {
            return pf.SyncFindPath(pos.x, pos.y, tx, ty, pfconfig);
        }
    }
}