using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Encounter
{
    public abstract class ActiveAbility : Ability
    {
                /// <summary>
        /// A version of BreakDownAbility but without returning a list of decisions but just the tiles
        /// that would be affected by the target.
        /// </summary>
        /// <param name="pn"></param>
        /// <returns></returns>
        public abstract List<PathNode> GetTargetTiles(int tx, int ty);
        /// <summary>
        /// AI Exclusive function - uses a synchronous caching system instead of regular threaded pathfinders.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract List<PathNode> GetTargetsFrom(int x, int y);
        /// <summary>
        /// AI Exclusive function - uses a synchronous caching system instead of regular threaded pathfinders.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        public abstract List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty);
        /// <summary>
        /// Get every tile which this ability is able to perform its ability on.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract List<PathNode> GetTilesWithinRange();
        public bool IsTargetWithinRange(int tx, int ty)
        {
            return GetTilesWithinRange().Exists(p => p.x == tx && p.y == ty);
        }
    }
}