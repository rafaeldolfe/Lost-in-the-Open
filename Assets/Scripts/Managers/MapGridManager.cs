using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MapGridManager : MonoBehaviour
{
    private GlobalEventManager gem;

    public MapGrid grid { get; set; }

    void Awake()
    {
        gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
        if (gem == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { gem };
            List<Type> depTypes = new List<Type> { typeof(GlobalEventManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }
    void Start()
    {
        gem.StartListening("Move", MoveActor);
        gem.StartListening("Death", RemoveActor);
    }
    void OnDestroy()
    {
        gem.StopListening("Move", MoveActor);
        gem.StopListening("Death", RemoveActor);
    }

    private void MoveActor(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (grid.GetGridObject(x, z) == null)
        {
            throw new System.Exception("Invalid grid coordinates");
        }
        if (grid.GetGridObject(tx, tz) == null)
        {
            throw new System.Exception("Invalid target grid coordinates");
        }
        if (grid.GetGridObject(x, z).actor == null)
        {
            throw new System.Exception(string.Format("Expected actor at position ({0}, {1}), but found null", x, z));
        }
        if (grid.GetGridObject(tx, tz).actor != null)
        {
            throw new System.Exception(string.Format("Expected empty position at ({0}, {1}), but found an actor", tx, tz));
        }
        grid.GetGridObject(x, z).RemoveActor();
        grid.GetGridObject(tx, tz).SetActor(invoker);
    }
    private void RemoveActor(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        grid.GetGridObject(x, z).RemoveActor();
    }
}
