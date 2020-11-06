using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadedPathfinding;
using Encounter;
using System.Linq;

public class MyCustomTileProvider : TileProvider
{
    public MapGrid grid;
    public MyCustomTileProvider(MapGrid grid) : base(grid.GetWidth(), grid.GetHeight())
    {
        this.grid = grid;
    }
    private float Abs(float x)
    {
        if (x < 0)
            return -x;
        else
            return x;
    }
    public override float GetCostToNeighbour(PNode a, PNode b)
    {
        // Only intended for neighbours.

        PathNode pn1 = grid.GetGridObject(a.X, a.Y).pn;
        PathNode pn2 = grid.GetGridObject(b.X, b.Y).pn;

        // Is directly horzontal
        if (Abs(a.X - b.X) == 1 && a.Y == b.Y)
        {
            return Encounter.Constants.MOVE_STRAIGHT_COST * pn2.walkIntoCost;
        }

        // Directly vertical.
        if (Abs(a.Y - b.Y) == 1 && a.X == b.X)
        {
            return Encounter.Constants.MOVE_STRAIGHT_COST * pn2.walkIntoCost;
        }

        // Assume that it is on one of the corners.
        return Encounter.Constants.MOVE_DIAGONAL_COST * pn2.walkIntoCost;
    }

    public override bool IsTileWalkable(int x, int y, PathfindingConfig pconf)
    {
        
        return grid.GetGridObject(x, y).IsTileWalkable(pconf);
    }

    public override List<PathNode> TranslatePath(List<PNode> pnodes)
    {
        if (pnodes == null)
        {
            return null;
        }
        return pnodes.Select(node => grid.GetGridObject(node.X, node.Y).pn).ToList();
    }
}
