using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    [RequireComponent(typeof(AbilitiesHandler))]
    public abstract class Ability : MonoBehaviour
    {
        public Color highlightColor { get; set; }
        [SerializeField]
        private Sprite image = null;

        [HideInInspector]
        public string category;
        [HideInInspector]
        public PathfindingConfig pfconfig;
        public float range;
        protected List<PathNode> tilesWithinRange = new List<PathNode>();

        public abstract bool Done();
        public abstract string Status();
        public abstract void Reset();
        public abstract IEnumerator UseAbility(List<PathNode> targets);
        /// <summary>
        /// Reason for BreakDownAbility having this complicated List of decisions, is because in the future there might be AOE abilities.
        /// These abilities may essentially perform 1 ability on several locations simultaneously.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract IEnumerator BreakDownAbility(int tx, int ty);
        /// <summary>
        /// Reason for BreakDownAbility having this complicated List of decisions, is because in the future there might be AOE abilities.
        /// These abilities may essentially perform 1 ability on several locations simultaneously.
        /// 
        /// This is the AI version of the ability which is synchronous.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract List<Decision> BreakDownAbility(List<PathNode> path);
        public abstract float GetRange();
        public Sprite GetSprite()
        {
            return image;
        }
    }

    public class PathfindingConfig
    {
        public bool ignoreAll;
        public bool ignoreLastTile;
        public bool ignoreActors;

        public PathfindingConfig(bool ignoreAll, bool ignoreLastTile, bool ignoreActors)
        {
            this.ignoreAll = ignoreAll;
            this.ignoreLastTile = ignoreLastTile;
            this.ignoreActors = ignoreActors;
        }

        public PathfindingConfig()
        {
            this.ignoreAll = false;
            this.ignoreLastTile = false;
            this.ignoreActors = true;
        }
    }
}