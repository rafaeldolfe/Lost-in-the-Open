using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class InputManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private MapGridManager mgm;
    private SelectedManager sm;

    void Awake()
    {
        gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
        sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager;
        mgm = FindObjectOfType(typeof(MapGridManager)) as MapGridManager;
        if (gem == null || sm == null || mgm == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, sm, mgm };
            List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(SelectedManager), typeof(MapGridManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }

    public void HandlePlayerInput()
    {

        if (sm.selected != null && sm.selected.ah.GetStatus() == "Busy")
        {
            return;
        }
        if (Input.GetKeyDown("l") && ProgramDebug.debug)
        {
            ProgramLog progLog = (FindObjectOfType(typeof(ProgramLog)) as ProgramLog);
            Debug.Log(progLog.GetLog());
        }
        for (int i = 1; i <= Constants.SIZE_OF_ABILITY_BAR; i++)
        {
            if (Input.GetKeyDown(i.ToString()))
            {
                sm.SetAbility(i - 1);
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            GridContainer current = mgm.grid.GetGridObject(ProgramUtils.GetMouseGridPosition().x, ProgramUtils.GetMouseGridPosition().y);

            if (current != null && current.actor != null)
            {
                sm.Select(current.actor);
            }
            else
            {
                sm.Unselect();
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            GridContainer current = mgm.grid.GetGridObject(ProgramUtils.GetMouseGridPosition().x, ProgramUtils.GetMouseGridPosition().y);

            if (current != null)
            {
                sm.UseAbility(current.x, current.z);
            }
        }
    }
}
