using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace WorldMap
{
    public class CheatManager : MonoBehaviour
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
            if (Input.GetKeyDown("w"))
            {
                gem.TriggerEvent("WinGame", gameObject, new List<object> { winScenario });
            }
        }
    }
}
