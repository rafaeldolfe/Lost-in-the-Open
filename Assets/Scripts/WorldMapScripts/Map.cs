using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using Newtonsoft.Json;

namespace WorldMap
{

    [Serializable]
    public class Map
    {
        public List<Location> locations;
        public List<Terrain> terrains;
        private readonly Dictionary<GameObject, Location> nodeToLoc;

        [JsonConstructor]
        public Map(List<Location> locations, List<Terrain> terrains)
        {
            this.locations = locations;
            this.terrains = terrains;
            nodeToLoc = new Dictionary<GameObject, Location>();
        }
        public void InstantiateMap(Func<GameObject, GameObject> instantiateFunc)
        {
            if (locations == null)
            {
                Debug.Log("list of locations is null");
                throw new Exception("list of locations is null");
            }
            for (int i = 0; i < locations.Count(); i++)
            {
                if (locations[i].id != i)
                {
                    Debug.Log($"Location id is not same as list index {locations[i].id} vs {i}");
                    throw new Exception($"Location id is not same as list index {locations[i].id} vs {i}");
                }
            }
            foreach (Location location in locations)
            {
                if (location.plainNode == null)
                {
                    nodeToLoc[location.InstantiateNode(instantiateFunc)] = location;
                }
                else
                {
                    nodeToLoc[location.InstantiatePlainNode(instantiateFunc)] = location;
                }
            }
            foreach (Terrain terrain in terrains)
            {
                Mesh mesh = terrain.plainMesh.GetUnityClass();
                Material material = terrain.tconfig.material;
                GameObject gameObject = new GameObject("regenerated_mesh" + terrain.tconfig.percentageOfMap, typeof(MeshFilter), typeof(MeshRenderer));

                gameObject.transform.localScale = new Vector3(1, 1, 1);
                gameObject.transform.position = Constants.TERRAIN_POSITIONS;

                terrain.gameObject = gameObject;
                gameObject.GetComponent<MeshFilter>().mesh = mesh;
                gameObject.GetComponent<MeshRenderer>().material = material;
            }
        }
        public Scenario GetScenario(GameObject node)
        {
            return nodeToLoc[node].scenario;
        }
        public Location GetLocation(GameObject node)
        {
            return nodeToLoc[node];
        }
        public List<Location> GetNeighboursOf(Location location)
        {
            return location.neighbours.Select(id => locations[id]).ToList();
        }

        public override string ToString()
        {
            if (locations == null || terrains == null)
            {
                string msg = "Map broken, ";
                if (locations == null)
                {
                    msg += "locations is null ";
                }
                if (terrains == null)
                {
                    msg += "terrains is null ";
                }
                throw new Exception(msg);
            }
            StringBuilder res = new StringBuilder("Terrains: \n");
            foreach (Terrain terrain in terrains)
            {
                res.Append(terrain);
            }

            res.Append("\n");
            res.Append("Locations: \n");

            foreach (Location location in locations)
            {
                res.Append(location);
            }

            return res.ToString();
        }
    }
}
