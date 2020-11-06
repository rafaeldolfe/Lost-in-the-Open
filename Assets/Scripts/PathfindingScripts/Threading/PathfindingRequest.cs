using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Encounter;

namespace ThreadedPathfinding
{
    public class PathfindingRequest
    {
        private static int idCounter = 0;
        private static Queue<PathfindingRequest> Pooled = new Queue<PathfindingRequest>();

        public int Id { get; private set; }
        public int StartX { get; private set; }
        public int StartY { get; private set; }
        public int EndX { get; private set; }
        public int EndY { get; private set; }
        public PathFound ReturnEvent { get; private set; }

        public List<PNode> ExistingList { get; private set; }
        public PathfindingConfig Config { get; private set; }
        public PathfindingResolution Resolution { get; private set; }

        public static PathfindingRequest Create(int startX, int startY, int endX, int endY, PathfindingConfig pconf = null, PathFound foundEvent = null, List<PNode> existingList = null)
        {
            PathfindingRequest r;
            if (Pooled.Count > 0)
            {
                r = Pooled.Dequeue();
                r.StartX = startX;
                r.StartY = startY;
                r.EndX = endX;
                r.EndY = endY;
                r.ReturnEvent = foundEvent;
                r.ExistingList = existingList;
                r.Config = pconf == null ? new PathfindingConfig() : pconf;
                r.Id = idCounter++;
                r.Resolution = new PathfindingResolution();
            }
            else
            {
                r = new PathfindingRequest(startX, startY, endX, endY, foundEvent, existingList, pconf);
            }

            if (PathfindingManager.Instance != null)
            {
                PathfindingManager.Instance.Enqueue(r);
            }

            return r;
        }

        private PathfindingRequest(int x, int y, int ex, int ey, PathFound foundEvent, List<PNode> existing = null, PathfindingConfig pconf = null)
        {
            this.StartX = x;
            this.StartY = y;
            this.EndX = ex;
            this.EndY = ey;
            this.ReturnEvent = foundEvent;
            this.ExistingList = existing;
            this.Config = pconf == null ? new PathfindingConfig() : pconf;
            this.Id = idCounter++;
            this.Resolution = new PathfindingResolution();
        }

        /// <summary>
        /// Returns the request to the pool. After calling this do not attempt to use this object again,
        /// instead request a new one.
        /// </summary>
        public void Dispose()
        {
            ReturnEvent = null;
            if (!Pooled.Contains(this))
                Pooled.Enqueue(this);
        }
    }
}
