using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class GameManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private MapGridManager mgm;
    private InputManager im;
    private SelectedManager sm;

    public GameObject actor;
    public GameObject pirate;
    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(mgm = FindObjectOfType(typeof(MapGridManager)) as MapGridManager);
        deps.Add(im = FindObjectOfType(typeof(InputManager)) as InputManager);
        deps.Add(sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager);
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        mgm.grid = new MapGrid(30, 30, 1, new Vector3(0, 0, 0), (MapGrid g, int x, int y) => new GridContainer(g, x, y));

        CreateActor(15, 5, actor);
        CreateActor(13, 8, actor);
        CreateActor(13, 9, actor);
        CreateActor(14, 8, actor);
        CreateActor(9, 5, actor);

        CreateActor(6, 2, pirate);
        CreateActor(7, 2, pirate);
        CreateActor(8, 2, pirate);
        CreateActor(9, 2, pirate);
        CreateActor(10, 2, pirate);
    }
    public void CreateActor(int x, int z, GameObject actor)
    {
        GameObject firstActor = UnityEngine.Object.Instantiate(actor, actor.transform.position + new Vector3(x, 0, z), Quaternion.identity);
        firstActor.GetComponent<Position>().init(x, z);
        mgm.grid.gridArray[x, z].SetActor(firstActor);
    }
    void Update()
    {
        im.HandlePlayerInput();
        sm.HandleSelectedActor();
    }
}
