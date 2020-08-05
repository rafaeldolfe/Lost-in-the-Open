using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;
using System.Runtime.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WorldMap
{
    [Serializable]
    public class TerrainConfig : IComparable<TerrainConfig>
    {
        [Tooltip("Name of terrain.")]
        public string name;
        [Tooltip("Available scenarios for this terrain with weights for the likelihood that each is chosen.")]
        [JsonIgnore]
        public Dictionary<Scenario, float> weightedScenarios;
        [JsonProperty]
        private List<KeyValuePair<Scenario, float>> WeightedScenarios
        {
            get { return weightedScenarios.ToList(); }
            set { weightedScenarios = value.ToDictionary(x => x.Key, x => x.Value); }
        }
        [Tooltip("The percentage of the map that should be covered by this terrain.")]
        [Range(0, 100)]
        public int percentageOfMap;
        [Tooltip("How much does this terrain prefer to be placed at the top of the map? e.g. mountain terrain has high position preference, meaning it wants to be put far up.")]
        public int positionPreference;
        [Tooltip("The texture material for this terrain.")]
        public PlainMaterial plainMaterial;
        [JsonIgnore]
        public Material material;

        [JsonConstructor]
        public TerrainConfig(string name, List<KeyValuePair<Scenario, float>> WeightedScenarios, int percentageOfMap, int positionPreference, PlainMaterial plainMaterial)
        {
            this.name = name;
            this.WeightedScenarios = WeightedScenarios;
            this.percentageOfMap = percentageOfMap;
            this.positionPreference = positionPreference;
            this.plainMaterial = plainMaterial;
            material = plainMaterial.GetUnityClass();
        }
        public TerrainConfig(string name, List<KeyValuePair<Scenario, float>> WeightedScenarios, int percentageOfMap, int positionPreference, Material material)
        {
            this.name = name;
            this.WeightedScenarios = WeightedScenarios;
            this.percentageOfMap = percentageOfMap;
            this.positionPreference = positionPreference;
            this.material = material;
            plainMaterial = material.GetPlainClass();
        }
        public int CompareTo(TerrainConfig tc)
        {
            return positionPreference.CompareTo(tc.positionPreference);
        }
        public override string ToString()
        {
            StringBuilder res = new StringBuilder("");

            res.Append($"Name: {name}\n");
            res.Append($"WeightedScenarios count: {WeightedScenarios.Count()}\n");
            res.Append($"Percentage Of Map: {percentageOfMap}\n");
            res.Append($"Position Preference: {positionPreference}\n");

            return res.ToString();
        }
    }
}

