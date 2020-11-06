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
using ThreadedPathfinding;

namespace Encounter
{
    public class CachedPath
    {
        public Ability ability;
        public List<PathNode> path;
        public int startX, startY, endX, endY;

        public CachedPath(Ability ability, List<PathNode> path, int startX, int startY, int endX, int endY)
        {
            this.ability = ability;
            this.path = path;
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
        }
    }
    public class Pathfinding : MonoBehaviour
    {

        private MapManager mgm;
        private GlobalEventManager gem;
        private PathfindingManager pfm;

        private MyCustomTileProvider mcp;

        private List<Vector2Int> directions;

        private MapGrid grid;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (pfm = FindObjectOfType(typeof(PathfindingManager)) as PathfindingManager),
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
        }
        void Start()
        {
            // Get a reference to the PathfindingManager object.
            PathfindingManager manager = PathfindingManager.Instance;

            // Now set the provider object. Replace MyCustomProvider with whatever you named your provider.
            grid = mgm.grid;
            manager.Provider = new MyCustomTileProvider(mgm.grid);
        }

        #region Player
        public List<PathNode> SyncFindPath(int startX, int startY, int endX, int endY, PathfindingConfig pconf)
        {
            return pfm.SyncFindPath(startX, startY, endX, endY, pconf);
        }
        public PathEnumerator AsyncFindPath(int startX, int startY, int endX, int endY, PathfindingConfig pconf)
        {
            return new PathEnumerator(pfm.GetPath(startX, startY, endX, endY, pconf));
        }
        /// <summary>
        /// This algorithm will always return at least starting PathNode.
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="range"></param>
        /// <param name="pfconfig"></param>
        /// <returns></returns>
        public List<PathNode> DijkstraWithinRange(int startX, int startY, float range, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;

            List<PathNode> openList = new List<PathNode> { startNode };
            List<PathNode> closedList = new List<PathNode>();

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

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);

                openList.Remove(currentNode);

                if (currentNode.gCost > range)
                {
                    continue;
                }

                closedList.Add(currentNode);

                List<PathNode> neighbours;


                neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

                foreach (PathNode neighbourNode in neighbours)
                {
                    if (closedList.Contains(neighbourNode)) continue;

                    float tentativeGCost = currentNode.gCost + GetCostToNeighbour(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }

            return closedList;
        }

        #endregion Player
        #region AI

        public void ResetCaches()
        {
            cachedDijkstraPaths.Clear();
            prevDijkstraCachings.Clear();
        }
        public List<PathNode> FindPathWithinRange(Ability ability, int startX, int startY, int endX, int endY)
        {
            return GetCached(ability, startX, startY, endX, endY);
        }
        public List<CachedPath> cachedDijkstraPaths = new List<CachedPath>();
        public List<(Ability, int, int, float)> prevDijkstraCachings = new List<(Ability, int, int, float)>();
        private void CachePaths(Ability ability, List<PathNode> nodes)
        {
            foreach (PathNode node in nodes)
            {
                List<PathNode> path = CalculatePath(node);
                PathNode first = path.First();
                PathNode last = path.Last();
                CachedPath cpath = new CachedPath(ability, path, first.x, first.y, last.x, last.y);
                cachedDijkstraPaths.Add(cpath);
            }
        }
        private List<PathNode> GetCached(Ability ability, int startX, int startY, int endX, int endY)
        {
            CachedPath cpath = cachedDijkstraPaths.Find(c => c.ability == ability && c.startX == startX && c.startY == startY && c.endX == endX && c.endY == endY);
            if (cpath == null)
            {
                return null;
            }
            else
            {
                return cpath.path;
            }
        }
        public List<PathNode> DijkstraWithinRangeCaching(Ability ability, int startX, int startY, float range, PathfindingConfig pfconfig = null)
        {
            if (pfconfig == null)
            {
                pfconfig = new PathfindingConfig();
            }

            PathNode startNode = grid.GetGridObject(startX, startY).pn;

            List<PathNode> openList = new List<PathNode> { startNode };
            List<PathNode> closedList = new List<PathNode>();

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

            while (openList.Count > 0)
            {
                PathNode currentNode = GetLowestFCostNode(openList);

                openList.Remove(currentNode);

                if (currentNode.gCost > range)
                {
                    continue;
                }

                closedList.Add(currentNode);

                List<PathNode> neighbours;


                neighbours = GetNeighbourListFiltered(currentNode, pfconfig);

                foreach (PathNode neighbourNode in neighbours)
                {
                    if (closedList.Contains(neighbourNode)) continue;

                    float tentativeGCost = currentNode.gCost + GetCostToNeighbour(currentNode, neighbourNode);
                    if (tentativeGCost < neighbourNode.gCost)
                    {
                        neighbourNode.cameFromNode = currentNode;
                        neighbourNode.gCost = tentativeGCost;

                        if (!openList.Contains(neighbourNode))
                        {
                            openList.Add(neighbourNode);
                        }
                    }
                }
            }


            if (!prevDijkstraCachings.Contains((ability, startX, startY, range)))
            {
                CachePaths(ability, closedList);
                prevDijkstraCachings.Add((ability, startX, startY, range));
            }

            return closedList;
        }

        #endregion AI

        public PathNode GetNode(int x, int y)
        {
            if (grid.GetGridObject(x,y) == null)
            {
                return null;
            }
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
        private bool left, right, below, above;
        private List<PathNode> GetNeighbourListFiltered(PathNode currentNode, PathfindingConfig pfconfig)
        {
            List<PathNode> neighbourList = new List<PathNode>();

            PathNode neighbour;

            left = false;
            neighbour = GetNode(currentNode.x - 1, currentNode.y);
            if (neighbour != null && neighbour.CheckConditions(pfconfig))
            {
                neighbourList.Add(neighbour);
                left = true;
            }

            // Right
            right = false;
            neighbour = GetNode(currentNode.x + 1, currentNode.y);
            if (neighbour != null && neighbour.CheckConditions(pfconfig))
            {
                neighbourList.Add(neighbour);
                right = true;
            }

            // Above
            above = false;
            neighbour = GetNode(currentNode.x, currentNode.y + 1);
            if (neighbour != null && neighbour.CheckConditions(pfconfig))
            {
                neighbourList.Add(neighbour);
                above = true;
            }

            // Below
            below = false;
            neighbour = GetNode(currentNode.x, currentNode.y - 1);
            if (neighbour != null && neighbour.CheckConditions(pfconfig))
            {
                neighbourList.Add(neighbour);
                below = true;
            }


            // Above-Left
            if (left && above)
            {
                neighbour = GetNode(currentNode.x - 1, currentNode.y + 1);
                if (neighbour != null && neighbour.CheckConditions(pfconfig))
                {
                    neighbourList.Add(neighbour);
                }
            }

            // Above-Right
            if (right && above)
            {
                neighbour = GetNode(currentNode.x + 1, currentNode.y + 1);
                if (neighbour != null && neighbour.CheckConditions(pfconfig))
                {
                    neighbourList.Add(neighbour);
                }
            }

            // Below-Left
            if (left && below)
            {
                neighbour = GetNode(currentNode.x - 1, currentNode.y - 1);
                if (neighbour != null && neighbour.CheckConditions(pfconfig))
                {
                    neighbourList.Add(neighbour);
                }
            }

            // Below-Right
            if (right && below)
            {
                neighbour = GetNode(currentNode.x + 1, currentNode.y - 1);
                if (neighbour != null && neighbour.CheckConditions(pfconfig))
                {
                    neighbourList.Add(neighbour);
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
        public int ManhattanDistance(int x, int y, int tx, int ty)
        {
            return Mathf.Abs(x - tx) + Mathf.Abs(y - ty);
        }
        private float CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int zDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - zDistance);
            return Constants.MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + Constants.MOVE_STRAIGHT_COST * remaining;
        }
        private float GetCostToNeighbour(PathNode a, PathNode b)
        {
            int xDistance = Mathf.Abs(a.x - b.x);
            int zDistance = Mathf.Abs(a.y - b.y);
            int remaining = Mathf.Abs(xDistance - zDistance);
            return Constants.MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) * b.walkIntoCost + Constants.MOVE_STRAIGHT_COST * remaining * b.walkIntoCost;
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