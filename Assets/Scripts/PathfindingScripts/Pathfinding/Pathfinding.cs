using Priority_Queue;
using System.Collections.Generic;
using UnityEngine;
using Encounter;
using System;

namespace ThreadedPathfinding.Internal
{
    public class Pathfinding
    {
        public const int MAX = 1000;
        public const float DIAGONAL_DST = 1.41421356237f;

        private FastPriorityQueue<PNode> open = new FastPriorityQueue<PNode>(MAX);
        private Dictionary<PNode, PNode> cameFrom = new Dictionary<PNode, PNode>();
        private Dictionary<PNode, float> costSoFar = new Dictionary<PNode, float>();
        private List<PNode> near = new List<PNode>();
        private bool left, right, below, above;

        public Pathfinding()
        {

        }

        public PathfindingResult Run(int startX, int startY, int endX, int endY, PathfindingConfig pconf, TileProvider provider, List<PNode> cachePath, out List<PNode> path)
        {
            if (provider == null)
            {
                path = null;
                return PathfindingResult.ERROR_INTERNAL;
            }

            // Validate start and end points.
            if (!provider.TileInBounds(startX, startY))
            {
                path = null;
                return PathfindingResult.ERROR_START_OUT_OF_BOUNDS;
            }
            if (!provider.TileInBounds(endX, endY))
            {
                path = null;
                return PathfindingResult.ERROR_END_OUT_OF_BOUNDS;
            }
            //Start can be not walkable, that's fine
            //if (!provider.IsTileWalkable(startX, startY))
            //{
            //    path = null;
            //    return PathfindingResult.ERROR_START_NOT_WALKABLE;
            //}
            //if (!pfconf.ignoreLastTile && !provider.IsTileWalkable(endX, endY))
            //{
            //    path = null;
            //    return PathfindingResult.ERROR_END_NOT_WALKABLE;
            //}

            // Clear everything up.
            Clear();

            var start = PNode.Create(startX, startY);
            var end = PNode.Create(endX, endY);

            // Check the start/end relationship 
            // IT IS NOW LEGAL TO TARGET WHERE START IS END.
            if (start.Equals(end))
            {
                path = new List<PNode> { start, end };
                return PathfindingResult.SUCCESSFUL;
            }

            // Add the starting point to all relevant structures.
            open.Enqueue(start, 0f);
            cameFrom[start] = start;
            costSoFar[start] = 0f;

            int count;
            while ((count = open.Count) > 0)
            {
                // Detect if the current open amount exceeds the capacity.
                // This only happens in very large open areas. Corridors and hallways will never cause this, not matter how large the actual path length.
                if (count >= MAX - 8)
                {
                    path = null;
                    return PathfindingResult.ERROR_PATH_TOO_LONG;
                }

                var current = open.Dequeue();

                if (current.Equals(end))
                {
                    // We found the end of the path!
                    path = TracePath(end, cachePath);
                    return PathfindingResult.SUCCESSFUL;
                }

                // Get all neighbours (tiles that can be walked on to)
                var neighbours = GetNear(current, provider, end, pconf);
                foreach (PNode n in neighbours)
                {
                    float newCost = costSoFar[current] + provider.GetCostToNeighbour(current, n); // Note that this could change depending on speed changes per-tile. Currently not implemented.

                    if (!costSoFar.ContainsKey(n) || newCost < costSoFar[n])
                    {
                        costSoFar[n] = newCost;
                        float priority = newCost + Heuristic(current, n);
                        open.Enqueue(n, priority);
                        cameFrom[n] = current;
                    }
                }
            }

            path = null;
            return PathfindingResult.ERROR_PATH_NOT_FOUND;
        }
        private bool CheckNodesEqual(int currX, int currY, int endX, int endY)
        {
            return currX == endX && currY == endY;
        }
        private List<PNode> TracePath(PNode end, List<PNode> path)
        {
            if (path == null)
                path = new List<PNode>();
            else
                path.Clear();
            PNode child = end;

            bool run = true;
            while (run)
            {
                PNode previous = cameFrom[child];
                path.Add(child);
                if (previous != null && child != previous)
                {
                    child = previous;
                }
                else
                {
                    run = false;
                }
            }

            path.Reverse();
            return path;
        }

        public void Clear()
        {
            costSoFar.Clear();
            cameFrom.Clear();
            near.Clear();
            open.Clear();
        }

        private float Abs(float x)
        {
            if (x < 0)
                return -x;
            else
                return x;
        }

        private float Heuristic(PNode a, PNode b)
        {
            // Gives a rough distance.
            return Abs(a.X - b.X) + Abs(a.Y - b.Y);
        }

        private float GetCost(PNode a, PNode b)
        {
            // Only intended for neighbours.

            // Is directly horzontal
            if (Abs(a.X - b.X) == 1 && a.Y == b.Y)
            {
                return 1;
            }

            // Directly vertical.
            if (Abs(a.Y - b.Y) == 1 && a.X == b.X)
            {
                return 1;
            }

            // Assume that it is on one of the corners.
            return DIAGONAL_DST;
        }

        private List<PNode> GetNear(PNode node, TileProvider provider, PNode end, PathfindingConfig pconf)
        {
            // Want to add nodes connected to the center node, if they are walkable.
            // This code stops the pathfinder from cutting corners, and going through walls that are diagonal from each other.

            near.Clear();
            // Left
            left = false;
            if (provider.TileInBounds(node.X - 1, node.Y) && provider.IsTileWalkable(node.X - 1, node.Y, pconf) || CheckNodesEqual(node.X - 1, node.Y, end.X, end.Y))
            {
                near.Add(PNode.Create(node.X - 1, node.Y));
                left = true;
            }

            // Right
            right = false;
            if (provider.TileInBounds(node.X + 1, node.Y) && provider.IsTileWalkable(node.X + 1, node.Y, pconf) || CheckNodesEqual(node.X + 1, node.Y, end.X, end.Y))
            {
                near.Add(PNode.Create(node.X + 1, node.Y));
                right = true;
            }

            // Above
            above = false;
            if (provider.TileInBounds(node.X, node.Y + 1) && provider.IsTileWalkable(node.X, node.Y + 1, pconf) || CheckNodesEqual(node.X, node.Y + 1, end.X, end.Y))
            {
                near.Add(PNode.Create(node.X, node.Y + 1));
                above = true;
            }

            // Below
            below = false;
            if (provider.TileInBounds(node.X, node.Y - 1) && provider.IsTileWalkable(node.X, node.Y - 1, pconf) || CheckNodesEqual(node.X, node.Y - 1, end.X, end.Y))
            {
                near.Add(PNode.Create(node.X, node.Y - 1));
                below = true;
            }

            bool includeDiagonals = true;

            // Above-Left
            if (includeDiagonals && left && above)
            {
                if (provider.TileInBounds(node.X - 1, node.Y + 1) && provider.IsTileWalkable(node.X - 1, node.Y + 1, pconf) || CheckNodesEqual(node.X - 1, node.Y + 1, end.X, end.Y))
                {
                    near.Add(PNode.Create(node.X - 1, node.Y + 1));
                }
            }

            // Above-Right
            if (includeDiagonals && right && above)
            {
                if (provider.TileInBounds(node.X + 1, node.Y + 1) && provider.IsTileWalkable(node.X + 1, node.Y + 1, pconf) || CheckNodesEqual(node.X + 1, node.Y + 1, end.X, end.Y))
                {
                    near.Add(PNode.Create(node.X + 1, node.Y + 1));
                }
            }

            // Below-Left
            if (includeDiagonals && left && below)
            {
                if (provider.TileInBounds(node.X - 1, node.Y - 1) && provider.IsTileWalkable(node.X - 1, node.Y - 1, pconf) || CheckNodesEqual(node.X - 1, node.Y - 1, end.X, end.Y))
                {
                    near.Add(PNode.Create(node.X - 1, node.Y - 1));
                }
            }

            // Below-Right
            if (includeDiagonals && right && below)
            {
                if (provider.TileInBounds(node.X + 1, node.Y - 1) && provider.IsTileWalkable(node.X + 1, node.Y - 1, pconf) || CheckNodesEqual(node.X + 1, node.Y - 1, end.X, end.Y))
                {
                    near.Add(PNode.Create(node.X + 1, node.Y - 1));
                }
            }

            return near;
        }
    }
}
