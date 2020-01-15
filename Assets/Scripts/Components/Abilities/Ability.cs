using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(AbilitiesHandler))]
public abstract class Ability : MonoBehaviour
{
    public Color highlightColor { get; set; }
    public Sprite image;

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
    public string GetAbilityName()
    {
        return this.GetType().ToString();
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
