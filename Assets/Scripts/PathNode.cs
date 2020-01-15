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

public class PathNode
{

    public GridContainer parent;
    public int x;
    public int z;

    public int gCost;
    public int hCost;
    public int fCost;

    public bool isWalkable;
    public bool hasActor;
    public PathNode cameFromNode;

    public PathNode(GridContainer parent, int x, int z)
    {
        this.parent = parent;
        this.x = x;
        this.z = z;
        isWalkable = true;
        hasActor = false;
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
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
        return x + "," + z;
    }

    public bool CheckConditions(PathfindingConfig pfconfig)
    {
        if (!pfconfig.ignoresActors && hasActor)
        {
            return false;
        }
        if (!pfconfig.ignoresTerrain && !isWalkable)
        {
            return false;
        }
        return true;
    }
}
