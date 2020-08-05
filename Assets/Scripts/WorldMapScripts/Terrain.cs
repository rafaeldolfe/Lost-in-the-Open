using UnityEngine;
using System;
using System.Collections;
using Utils;
using Newtonsoft.Json;

namespace WorldMap
{
    [Serializable]
    public class Terrain
    {
        public TerrainConfig tconfig;
        public PlainMesh plainMesh;
        [JsonIgnore]
        public GameObject gameObject;

        [JsonConstructor]
        public Terrain(TerrainConfig tconfig, PlainMesh plainMesh)
        {
            this.tconfig = tconfig;
            this.plainMesh = plainMesh;
        }
        public Terrain(TerrainConfig tconfig, Mesh mesh)
        {
            this.tconfig = tconfig;
            this.plainMesh = mesh.GetPlainClass();
        }
        public override string ToString()
        {
            return $"TerrainConfig: {tconfig}";
        }
    }
}
