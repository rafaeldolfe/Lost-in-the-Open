using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridContainer
{
    public List<GameObject> gos;
    public GameObject actor;
    public GameObject floor;
    private MapGrid grid;
    public PathNode pn;
    public int x;
    public int z;

    public float height = 1.0f;

    public GridContainer(MapGrid grid, int x, int z)
    {
        this.grid = grid;
        this.x = x;
        this.z = z;
        this.gos = new List<GameObject>(10);
        this.pn = new PathNode(this, x, z);
    }

    public void SetFloor(GameObject actor, float height)
    {
        this.floor = actor;
        this.height = height;
    }

    public void SetActor(GameObject actor)
    {
        this.actor = actor;
        pn.SetHasActor(true);
    }

    public void RemoveActor()
    {
        this.actor = null;
        pn.SetHasActor(false);
    }

    public void AddGameObject(GameObject gameObject)
    {
        gos.Add(gameObject);
    }

    public void RemoveGameObject(GameObject gameObject)
    {
        gos.Remove(gameObject);
    }

    public override string ToString()
    {
        return x + "," + z;
    }
}
