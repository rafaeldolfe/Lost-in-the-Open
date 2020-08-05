using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;
using WorldMap;
using Sirenix.OdinInspector;

namespace Encounter
{
    public class CheatManager : MonoBehaviour
    {
        private GlobalEventManager gem;

        public GameObject playerKing;
        public List<GameObject> playerUnits;
        public List<GameObject> enemies;

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
        }
        [Button]
        public void TriggerScenario()
        {
            gem.TriggerEvent("SpawnEnemyUnits", gameObject, new List<object> { enemies, new List<GameObject> { } });
            gem.TriggerEvent("SpawnPlayerUnits", gameObject, new List<object> { playerUnits, new List<GameObject> { }, playerKing });
        }
        // Update is called once per frame
        void Update()
        {

            if (Input.GetKeyDown("e"))
            {
                gem.TriggerEvent("CHEAT_EndTurn", gameObject);
            }
        }
    }
}
