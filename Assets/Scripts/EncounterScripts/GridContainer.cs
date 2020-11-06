using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public class GridContainer
    {
        public List<GameObject> gos;
        public GameObject actor;
        public GameObject floor;
        private MapGrid grid;
        public PathNode pn;
        public int x;
        public int y;

        public float height = 0.0f;

        public GridContainer(MapGrid grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            this.gos = new List<GameObject>(10);
            this.pn = new PathNode(this, x, y);
        }

        public bool IsTileWalkable(PathfindingConfig pconf)
        {
            if (!pconf.ignoreActors && pn.hasActor)
            {
                return false;
            }
            if (!pconf.ignoreAll && !pn.isWalkable)
            {
                return false;
            }
            return true;
        }

        public void SetActor(GameObject actor)
        {
            this.actor = actor;
            pn.SetHasActor(true);
        }

        public void RemoveActor()
        {
            this.actor = null;
            pn.SetHasActor(false);
        }

        public void AddGameObject(GameObject gameObject)
        {
            gos.Add(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            gos.Remove(gameObject);
        }

        public override string ToString()
        {
            return x + "," + y;
        }
    }
}