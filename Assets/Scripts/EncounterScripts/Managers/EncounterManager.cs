using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;


namespace Encounter
{
    public class EncounterManager : MonoBehaviour
    {

        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;

        private IEnumerable<GameObject> playerUnits = new List<GameObject>();
        private GameObject king;
        private IEnumerable<GameObject> enemyUnits = new List<GameObject>();
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("GenerateEncounter", GenerateEncounter);
            gem.StartListening("RegenerateScene", RegenerateEncounter);

            gem.StartListening("RegisterPlayerUnits", RegisterPlayerUnits);
            gem.StartListening("RegisterEnemyUnits", RegisterEnemyUnits);
            gem.StartListening("PrepareQuitToMainMenu", SaveEncounter);
            gem.StartListening("PrepareQuitToDesktop", SaveEncounter);
            gem.StartListening("EndAITurn", SaveEncounter);

            gem.StartListening("Death", CheckEncounterStatus);
        }
        private void OnDestroy()
        {
            gem.StopListening("GenerateEncounter", GenerateEncounter);
            gem.StopListening("RegenerateScene", RegenerateEncounter);

            gem.StopListening("RegisterPlayerUnits", RegisterPlayerUnits);
            gem.StopListening("RegisterEnemyUnits", RegisterEnemyUnits);
            gem.StopListening("PrepareQuitToMainMenu", SaveEncounter);
            gem.StopListening("PrepareQuitToDesktop", SaveEncounter);
            gem.StopListening("EndAITurn", SaveEncounter);

            gem.StopListening("Death", CheckEncounterStatus);
        }
        private void CheckEncounterStatus(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (king == invoker)
            {
                gem.TriggerEvent("GameOver", gameObject);
            }
            enemyUnits = enemyUnits.Where(unit => unit != null);
            if (enemyUnits.Count() == 1 && enemyUnits.ToList()[0] == invoker)
            {
                gem.TriggerEvent("EncounterWin", gameObject);
            }
        }
        private void SaveEncounter()
        {
            gdm.SetGameData("PlayerKing", king.GetPlainClass());
            gdm.SetGameData("PlayerUnits", playerUnits.GetPlainClasses());
            gdm.SetGameData("EnemyUnits", enemyUnits.GetPlainClasses());
            gdm.Save();
        }
        private void GenerateEncounter(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected list with 1 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<PlainGameObject>, found ", parameters[0].GetType()));
            }
            PlainGameObject savedKing = gdm.GetGameData<PlainGameObject>("PlayerKing");
            List<PlainGameObject> savedPlayerUnits = gdm.GetGameData<List<PlainGameObject>>("PlayerUnits");
            List<PlainGameObject> encounteredEnemyUnits = (List<PlainGameObject>) parameters[0];

            gem.TriggerEvent("SpawnPlayerUnits", gameObject, new List<object> { savedPlayerUnits, new List<PlainGameObject> { }, savedKing });
            gem.TriggerEvent("SpawnEnemyUnits", gameObject, new List<object> { encounteredEnemyUnits, new List<PlainGameObject> { } });

            SaveEncounter();
        }
        private void RegenerateEncounter()
        {
            PlainGameObject savedKing = gdm.GetGameData<PlainGameObject>("PlayerKing");
            List<PlainGameObject> savedPlayerUnits = gdm.GetGameData<List<PlainGameObject>>("PlayerUnits");
            List<PlainGameObject> savedEnemyUnits = gdm.GetGameData<List<PlainGameObject>>("EnemyUnits");

            gem.TriggerEvent("RespawnPlayerUnits", gameObject, new List<object> { savedPlayerUnits, savedKing });
            gem.TriggerEvent("RespawnEnemyUnits", gameObject, new List<object> { savedEnemyUnits });

            SaveEncounter();
        }
        private void RegisterPlayerUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new Exception(string.Format("Expected list with 3 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<GameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<GameObject>, found ", parameters[0].GetType()));
            }
            if (parameters[1].GetType() != typeof(GameObject))
            {
                throw new Exception(string.Format("Expected 2nd item to be GameObject, found ", parameters[2].GetType()));
            }
            List<GameObject> units = (List<GameObject>)parameters[0];
            GameObject king = (GameObject)parameters[1];

            playerUnits = units;
            this.king = king;
        }
        private void RegisterEnemyUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected list with 1 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<GameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<GameObject>, found ", parameters[0].GetType()));
            }
            List<GameObject> units = (List<GameObject>)parameters[0];

            enemyUnits = units;

            gdm.SetGameData("EnemyUnits", enemyUnits.GetPlainClasses());
        }
        public List<GameObject> _GetPlayerUnits()
        {
            return playerUnits.ToList();
        }
        public List<GameObject> _GetEnemyUnits()
        {
            return enemyUnits.ToList();
        }
        public GameObject _GetKing()
        {
            return king;
        }
    }
}
