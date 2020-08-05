using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
using Sirenix.OdinInspector;

namespace WorldMap
{
    
    [Serializable]
    [CreateAssetMenu]
    public class TerrainConfigScriptableObject : SerializedScriptableObject
    {
        [Tooltip("Name of terrain.")]
        public string name;
        [Tooltip("Available scenarios for this terrain with weights for the likelihood that each is chosen.")]
        [SerializeField]
        public Dictionary<ScenarioScriptableObject, float> weightedScenarios;
        [Tooltip("The percentage of the map that should be covered by this terrain.")]
        [Range(0,100)]
        public int percentageOfMap;
        [Tooltip("How much does this terrain prefer to be placed at the top of the map? e.g. mountain terrain has high position preference, meaning it wants to be put far up.")]
        public int positionPreference;
        [Tooltip("The texture material for this terrain.")]
        public Material material;

        public TerrainConfig GetPlainClass()
        {
            List<KeyValuePair<Scenario, float>> weightedScenariosTemp = weightedScenarios
                .Select(pair => new KeyValuePair<Scenario, float>(pair.Key.GetPlainClass(), pair.Value))
                .ToList();

            return new TerrainConfig(name, weightedScenariosTemp, percentageOfMap, positionPreference, material);
        }
    }
}

