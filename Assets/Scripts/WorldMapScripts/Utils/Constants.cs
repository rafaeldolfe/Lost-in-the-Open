using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WorldMap
{
    public class Constants : MonoBehaviour
    {
        public static string NODE_PREFAB_FOLDER = "Prefabs/Nodes";
        public static string NODE_CONTAINER_NAME = "NodeContainer";
        public const int STARTING_GOLD = 10;
        public const int STARTING_FOOD = 10;
        public const int NUMBER_OF_VERTICES_IN_TERRAINS = 10;
        public const int NUMBER_OF_WIN_LOCATIONS = 3;
        public const float TERRAIN_VOLUME_VARIATION = 1.0f;
        public const int VORONOI_POLYGON_NUMBER = 10;
        public static Color NODE_HIGHLIGHT = new Color(0.5f, 0.5f, -0.5f);
        public static Color COLOR_GOLD = new Color(1, 0.91f, 0.2f);
        public static Color COLOR_FOOD = new Color(0.37f, 0.95f, 0.2f);
        public static Vector3 TERRAIN_POSITIONS = new Vector3(0, 0, -1);
    }
}
