using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;

public class Pathfinding
{
    private BoatGrid<PathNode> grid;

    public Pathfinding(int width, int height)
    {
        //grid = new GridScript<int>(18, 6, 0.18f + -0.005f, min + new Vector3(0.1f, 0.1f, -1f) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0.01f, 0.01f, 0) + new Vector3(0, 0.01f, 0), (GridScript<int> g, int x, int y) => 0);
        grid = new BoatGrid<PathNode>(width, height, 10f, Vector3.zero, (BoatGrid<PathNode> g, int x, int y) => new PathNode(g, x, y));
    }
}
