using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decision
{
    public Ability ability;
    public List<PathNode> path;

    public Decision(Ability ability, List<PathNode> path)
    {
        this.ability = ability;
        this.path = path;
    }
}

public abstract class Behaviour : MonoBehaviour
{
    public abstract Analysis GetAnalysis(List<Ability> abilities);
}