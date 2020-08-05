using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using Newtonsoft.Json;
using Utils;


namespace WorldMap
{
    [Serializable]
    public class Location
    {
        public int id;
        public string terrainType;
        public Scenario scenario;
        public Vector2f position;
        public List<int> neighbours;
        public PlainGameObject plainNode;
        [JsonIgnore]
        public GameObject node;

        [JsonConstructor]
        public Location(int id, string terrainType, Scenario scenario, Vector2f position, PlainGameObject plainNode)
        {
            this.id = id;
            this.terrainType = terrainType;
            this.scenario = scenario;
            this.position = position;
            this.plainNode = plainNode;
        }
        public Location(int id, string terrainType, Scenario scenario, Vector2f position)
        {
            this.id = id;
            this.terrainType = terrainType;
            this.scenario = scenario;
            this.position = position;
        }
        public GameObject InstantiateNode(Func<GameObject, GameObject> instantiateFunc)
        {
            GameObject iconPrefab = Resources.Load($"{Constants.NODE_PREFAB_FOLDER}/{scenario.iconPrefabName}") as GameObject;
            Transform nodeContainer = GameObject.Find(Constants.NODE_CONTAINER_NAME).transform;
            node = instantiateFunc(iconPrefab);
            node.transform.position = new Vector3(position.x, position.y, 0);
            node.transform.position = new Vector3(position.x, position.y, 0);
            node.transform.SetParent(nodeContainer);
            return node;
        }
        public GameObject InstantiatePlainNode(Func<GameObject, GameObject> instantiateFunc)
        {
            node = plainNode.InstantiateSelf(instantiateFunc);
            node.transform.position = new Vector3(position.x, position.y);
            return node;
        }
        public void ChangeScenario(Scenario scenario, Func<GameObject, GameObject> instantiateFunc, Action<GameObject> destroyFunc)
        {
            if (node == null)
            {
                this.scenario = scenario;
            }
            else
            {
                this.scenario = scenario;
                destroyFunc(node);
                plainNode = null;
                InstantiateNode(instantiateFunc);
            }
        }
        public override string ToString()
        {
            StringBuilder res = new StringBuilder("");

            res.Append($"id: {id}\n");
            res.Append($"Position: {position}\n");
            res.Append($"Scenario: {scenario}\n");

            return res.ToString();
        }
    }
}
