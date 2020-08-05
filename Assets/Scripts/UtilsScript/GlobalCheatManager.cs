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
        private ScenarioScriptableObject _scenario;
        [SerializeField]
        private ScenarioScriptableObject winScenario;
        private Scenario scenario;

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
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("j"))
            {
                gem.TriggerEvent("AddGold", gameObject, new List<object> { 1 });
            }
            if (Input.GetKeyDown("k"))
            {
                gem.TriggerEvent("AddFood", gameObject, new List<object> { 1 });
            }
            if (Input.GetKeyDown("n"))
            {
                gem.TriggerEvent("AddGold", gameObject, new List<object> { -1 });
            }
            if (Input.GetKeyDown("m"))
            {
                gem.TriggerEvent("AddFood", gameObject, new List<object> { -1 });
            }
            if (Input.GetKeyDown("e"))
            {
                gem.TriggerEvent("StartScenario", gameObject, new List<object> { scenario });
            }
            if (Input.GetKeyDown("y"))
            {
                gem.TriggerEvent("WinGame", gameObject, new List<object> { winScenario });
            }
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Debug.Log("Deleting...");
                SelectedManager sm = FindObjectOfType(typeof(SelectedManager)) as SelectedManager;
                Position pos = sm.selected.go.GetComponent<Position>();
                Debug.Log($"pos.x: {pos.x}, pos.y: {pos.y}");
                gem.TriggerEvent("Attack", gameObject, new List<object> { 999 }, pos.x, pos.y, pos.x, pos.y);
            }
        }
    }
}
