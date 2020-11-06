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
        private FactionManager fm;
        private AIManager aim;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager),
                (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
                (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
                (aim = FindObjectOfType(typeof(AIManager)) as AIManager),
            };
            if (deps.Contains(null))
            {
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

            if (sm.selected != null)
            {
                GridContainer current = mgm.grid.GetGridObject(ProgramUtils.GetMouseGridPosition().x, ProgramUtils.GetMouseGridPosition().y);

                sm.HighlightTargetTile(current);
            }
        }
    }
}