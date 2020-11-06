using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WorldMap;
using Encounter;

namespace Utils
{
    public class GlobalCheatManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        [SerializeField]
        private ScenarioScriptableObject _scenario = null;
        [SerializeField]
        private ScenarioScriptableObject winScenario = null;
        [SerializeField]
        private ScenarioScriptableObject _scenario_mountain_encounter = null;
        [SerializeField]
        private ScenarioScriptableObject _scenario_coast_encounter = null;
        private Scenario scenario;
        private Scenario scenario_mountain_encounter;
        private Scenario scenario_coast_encounter;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            scenario = _scenario.GetPlainClass();
            scenario_mountain_encounter = _scenario_mountain_encounter.GetPlainClass();
            scenario_coast_encounter = _scenario_coast_encounter.GetPlainClass();
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("j"))
            {
                gem.TriggerEvent("AddGold", gameObject, new List<object> { 10 });
            }
            if (Input.GetKeyDown("k"))
            {
                gem.TriggerEvent("AddFood", gameObject, new List<object> { 10 });
            }
            if (Input.GetKeyDown("n"))
            {
                gem.TriggerEvent("AddGold", gameObject, new List<object> { -10 });
            }
            if (Input.GetKeyDown("m"))
            {
                gem.TriggerEvent("AddFood", gameObject, new List<object> { -10 });
            }
            if (Input.GetKeyDown("e"))
            {
                gem.TriggerEvent("StartScenario", gameObject, new List<object> { scenario });
            }
            if (Input.GetKeyDown("r"))
            {
                gem.TriggerEvent("StartScenario", gameObject, new List<object> { scenario_mountain_encounter });
            }
            if (Input.GetKeyDown("t"))
            {
                gem.TriggerEvent("StartScenario", gameObject, new List<object> { scenario_coast_encounter });
            }
            if (Input.GetKeyDown("y"))
            {
                gem.TriggerEvent("WinGame", gameObject, new List<object> { winScenario });
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                SelectedManager sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager;
                Position pos = sm.selected.go.GetComponent<Position>();
                gem.TriggerEvent("Attack", gameObject, new List<object> { 999 }, pos.x, pos.y, pos.x, pos.y);
            }
        }
    }
}
