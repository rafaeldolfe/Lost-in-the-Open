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

namespace Encounter
{
    public class PathNode
    {
        public GridContainer parent;
        public int x;
        public int y;

        public bool explored = false;
        public int playerVisionCounter = 0;
        public int enemyVisionCounter = 0;

        #region Pathfinding
        /// <summary>
        /// The cost of walking into this tile. The higher the value, the more rough the terrain is. Default is 1.
        /// </summary>
        public float walkIntoCost = 1;
        /// <summary>
        /// The path taken to get to this node
        /// </summary>
        public List<PathNode> path = new List<PathNode>();
        public PathNode cameFromNode;

        public float hCost;
        /// <summary>
        /// Cost from this node to end node
        /// </summary>
        public float gCost = 99999999;
        /// /// <summary>
        /// Cost from start node to this node
        /// </summary>
        public float fCost
        {
            get { return hCost + gCost; }
        }
        #endregion

        public bool isWalkable;
        public bool hasActor;

        public PathNode(GridContainer parent, int x, int y)
        {
            this.parent = parent;
            this.x = x;
            this.y = y;
            isWalkable = true;
            hasActor = false;
        }

        public void SetIsWalkable(bool isWalkable)
        {
            this.isWalkable = isWalkable;
        }

        public void SetHasActor(bool hasUnit)
        {
            this.hasActor = hasUnit;
        }

        public override string ToString()
        {
            return x + "," + y;
        }

        public string DeepToString()
        {
            return $"gCost: {gCost}, path: {path}, cameFromNode: {cameFromNode}, isWalkable: {isWalkable}, hasActor: {hasActor}" + y;
        }

        public bool CheckConditions(PathfindingConfig pfconfig)
        {
            if (!pfconfig.ignoreActors && hasActor)
            {
                return false;
            }
            if (!pfconfig.ignoreAll && !isWalkable)
            {
                return false;
            }
            return true;
        }
    }
}
