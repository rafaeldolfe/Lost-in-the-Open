using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class PathNode
{
    public enum STATUS { NORMAL, UNAVAILABLE, HIGHLIGHTED };
    private GridScript<PathNode> grid;
    private int x;
    private int y;
    private STATUS status;

    public int gCost;
    public int hCost;
    public int fCost;

    public PathNode cameFromNode;

    public PathNode(GridScript<PathNode> grid, int x, int y)
    {
        this.grid = grid;
        this.x = x;
        this.y = y;
        this.status = STATUS.NORMAL;
    }

    public override string ToString()
    {
        return x + "," + y;
    }

    public STATUS GetStatus()
    {
        return status;
    }

    public void SetStatus(STATUS status)
    {
        this.status = status;
    }
}
