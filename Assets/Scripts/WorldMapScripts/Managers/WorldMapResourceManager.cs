using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Utils;
using Sirenix.OdinInspector;

namespace WorldMap
{
    public class WorldMapResourceManager : MonoBehaviour
    {
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI foodText;

        private int gold = 0;
        private int food = 0;
        private List<PlainGameObject> playerUnits;

        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;

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
            gem.StartListening("AddGold", AddGold);
            gem.StartListening("AddFood", AddFood);
            gem.StartListening("AddUnits", AddUnits);
        }
        private void Start()
        {
            food = gdm.GetGameData<int>("Food");
            gold = gdm.GetGameData<int>("Gold");
            playerUnits = gdm.GetGameData<List<PlainGameObject>>("PlayerUnits") ?? new List<PlainGameObject>();
            UpdateTexts();
        }
        private void OnDestroy()
        {
            gem.StopListening("AddGold", AddGold);
            gem.StopListening("AddFood", AddFood);
            gem.StopListening("AddUnits", AddUnits);
        }
        private void EatFood()
        {
            Debug.Log("Fak ur mader!");
            int unitCount = gdm.GetGameData<List<PlainGameObject>>("PlayerUnits").Count;
            AddFood(-(unitCount * Constants.UNIT_FOOD_EAT_COST + Constants.KING_FOOD_EAT_COST));
            Debug.Log($"unitCount: {unitCount}");
            Debug.Log($"addfood: {-unitCount * Constants.UNIT_FOOD_EAT_COST}");
            UpdateTexts();
        }
        private void AddGold(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected gold amount, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(int))
            {
                throw new Exception(string.Format("Expected first object to be int, found {0}", parameters[0].GetType()));
            }
            AddGold((int)parameters[0]);
        }
        private void AddGold(int amount)
        {
            gold += amount;
            gdm.SetGameData("Gold", gold);
            UpdateTexts();
        }
        private void AddFood(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected food amount, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(int))
            {
                throw new Exception(string.Format("Expected first object to be int, found {0}", parameters[0].GetType()));
            }
            AddFood((int)parameters[0]);
        }
        private void AddFood(int amount)
        {
            food += amount;
            gdm.SetGameData("Food", food);
            UpdateTexts();
        }
        private void AddUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected food amount, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected first object to be int, found {0}", parameters[0].GetType()));
            }
            List<PlainGameObject> plainGameObjects = (List<PlainGameObject>)parameters[0];
            AddUnits(plainGameObjects);
        }
        private void AddUnits(List<PlainGameObject> plainGameObjects)
        {
            playerUnits.AddRange(plainGameObjects);
            gdm.SetGameData("PlayerUnits", playerUnits);
            UpdateTexts();
        }
        private void UpdateTexts()
        {
            goldText.text = $"Gold: {gold}";
            foodText.text = $"Food: {food}";
        }
        [Button]
        private void SetGold(int add = 1)
        {
            gold += add;
            gdm.SetGameData("Gold", gold);
        }
        [Button]
        private void GetGold()
        {
            Debug.Log("Gold");
            Debug.Log(gdm.GetGameData<int>("Gold"));
        }
    }
}
