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
        private Sprite image;

        [HideInInspector]
        public string category;
        [HideInInspector]
        public PathfindingConfig pfconfig;
        public int range;

        public abstract bool Done();
        public abstract string Status();
        public abstract void Reset(List<object> parameters);
        public abstract void UseAbility(List<PathNode> path);
        public abstract int GetRange();
        public abstract List<PathNode> GetTargetsFrom(int x, int y);
        public abstract List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty);
        public string GetAbilityName()
        {
            return this.GetType().ToString();
        }
        public Sprite GetSprite()
        {
            return image;
        }
    }

    public class PathfindingConfig
    {
        public bool ignoresTerrain;
        public bool ignoresActors;

        public PathfindingConfig(bool ignoresTerrain, bool ignoresActors)
        {
            this.ignoresTerrain = ignoresTerrain;
            this.ignoresActors = ignoresActors;
        }
        public PathfindingConfig()
        {
            this.ignoresTerrain = false;
            this.ignoresActors = true;
        }
    }
}