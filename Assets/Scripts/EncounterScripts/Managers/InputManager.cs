using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    public class InputManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private MapManager mgm;
        private SelectedManager sm;

        void Awake()
        {
            gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
            sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager;
            mgm = FindObjectOfType(typeof(MapManager)) as MapManager;
            if (gem == null || sm == null || mgm == null)
            {
                List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, sm, mgm };
                List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(SelectedManager), typeof(MapManager) };
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
        }

        private void Update()
        {
            if (PauseService.IsLevelPaused(PauseService.MENU_PAUSE))
            {
                return;
            }
            if (sm.selected != null && sm.selected.ah.GetStatus() == "Busy")
            {
                return;
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
                    sm.UseAbility(current.x, current.y);
                }
            }
        }
    }
}