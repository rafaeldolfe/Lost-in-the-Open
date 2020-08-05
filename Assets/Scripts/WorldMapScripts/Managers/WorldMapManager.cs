using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using csDelaunay;
using System.Linq;
using System.Text;
using System;
using Utils;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace WorldMap
{
    public class WorldMapManager : MonoBehaviour
    {
        public List<TerrainConfigScriptableObject> terrainConfigScriptableObjects;
        public ScenarioScriptableObject winScenario;

        private class MapFineTuner
        {
            public void PlaceWinScenarios(Map map, Scenario winScenario)
            {
                List<Location> possibleWinLocations = map.locations;
                possibleWinLocations = possibleWinLocations
                    .OrderBy(loc => loc.position.x)
                    .Take(Constants.NUMBER_OF_WIN_LOCATIONS)
                    .ToList();
                if (possibleWinLocations.Count() < Constants.NUMBER_OF_WIN_LOCATIONS)
                {
                    throw new Exception($"Could not find {Constants.NUMBER_OF_WIN_LOCATIONS} locations close to end");
                }
                for (int i = 0; i < Constants.NUMBER_OF_WIN_LOCATIONS; i++)
                {
                    possibleWinLocations[i].ChangeScenario(winScenario, Instantiate, Destroy);
                }
            }
        }
        private class MapGenerator
        {
            public Map GenerateMap(List<Location> locations, List<Terrain> terrains)
            {
                return new Map(locations, terrains);
            }
        }
        private class TerrainGenerator
        {
            /// <summary>
            /// Generates terrains within the bounds of MapUnderlay. The vertices go from left to right, up, and then right to left. 
            /// It starts with the lowest priority terrain.
            /// The reason for the convoluted vertex direction is in order to be able to detect if a point is within the terrain.
            /// </summary>
            /// <param name="tconfigs"></param>
            /// <param name="miny"></param>
            /// <param name="maxy"></param>
            /// <param name="minx"></param>
            /// <param name="maxx"></param>
            /// <returns></returns>
            public List<Terrain> GenerateTerrains(List<TerrainConfigScriptableObject> scriptableTerrainConfigs, float miny, float maxy, float minx, float maxx)
            {
                List<TerrainConfig> tconfigs = scriptableTerrainConfigs.Select(t => t.GetPlainClass()).ToList();
                List<Terrain> terrains = new List<Terrain>();
                tconfigs.Sort();

                float currenty = miny;
                float currentx = minx;

                float step = Math.Abs(minx - maxx) / Constants.NUMBER_OF_VERTICES_IN_TERRAINS;

                Vector3[] vertices = new Vector3[Constants.NUMBER_OF_VERTICES_IN_TERRAINS + 1];
                for (int i = 0; i < Constants.NUMBER_OF_VERTICES_IN_TERRAINS + 1; i++)
                {
                    vertices[i] = new Vector3(currentx, currenty);
                    currentx += step;
                }

                for (int tci = 0; tci < tconfigs.Count(); tci++)
                {
                    TerrainConfig tc = tconfigs[tci];

                    float heightOfTerrain = Math.Abs(miny - maxy) * ((float)tc.percentageOfMap / 100);

                    Vector3[] nextVertices = new Vector3[2 * (Constants.NUMBER_OF_VERTICES_IN_TERRAINS + 1)];
                    Vector2[] nextUVs = new Vector2[2 * (Constants.NUMBER_OF_VERTICES_IN_TERRAINS + 1)];
                    int[] nextTriangles = new int[6 * (Constants.NUMBER_OF_VERTICES_IN_TERRAINS + 1) - 6];

                    Array.Copy(vertices, 0, nextVertices, 0, vertices.Count());


                    // If it is the last iteration
                    if (tci == tconfigs.Count() - 1)
                    {
                        currenty = maxy;
                        for (int i = vertices.Count(); i < nextVertices.Count(); i++)
                        {
                            Vector3 vertex = vertices[i - vertices.Count()];
                            int jumpIndex = nextVertices.Count() - 1 - (i - vertices.Count());
                            nextVertices[jumpIndex] = new Vector3(vertex.x, currenty);
                            // TODO, ADD UV mapping
                        }
                    }
                    else
                    {
                        for (int i = vertices.Count(); i < nextVertices.Count(); i++)
                        {
                            Vector3 vertex = vertices[i - vertices.Count()];
                            int jumpIndex = nextVertices.Count() - 1 - (i - vertices.Count());
                            float borderVariation = UnityEngine.Random.Range(-Constants.TERRAIN_VOLUME_VARIATION, Constants.TERRAIN_VOLUME_VARIATION);
                            nextVertices[jumpIndex] = new Vector3(vertex.x, vertex.y + heightOfTerrain + borderVariation);
                            // TODO, ADD UV mapping
                        }
                    }

                    for (int i = nextVertices.Count() - 1; i > vertices.Count(); i--)
                    {
                        int ti = ((nextVertices.Count() - 1) - i) * 6;
                        int jumpIndex = nextVertices.Count() - 1 - (i - vertices.Count());
                        nextTriangles[ti] = (nextVertices.Count() - 1) - i;
                        nextTriangles[ti + 1] = (nextVertices.Count() - 1) - i + 1;
                        nextTriangles[ti + 2] = i;
                        nextTriangles[ti + 3] = i;
                        nextTriangles[ti + 4] = (nextVertices.Count() - 1) - i + 1;
                        nextTriangles[ti + 5] = i - 1;
                    }

                    Mesh mesh = new Mesh
                    {
                        vertices = nextVertices,
                        uv = nextUVs,
                        triangles = nextTriangles
                    };

                    terrains.Add(new Terrain(tc, mesh));

                    Array.Copy(nextVertices, vertices.Count(), vertices, 0, vertices.Count());
                    vertices = vertices.Reverse().ToArray();
                }
                return terrains;
            }
        }
        private class LocationsGenerator
        {
            public List<Location> GenerateLocations(List<Site> sites, List<Terrain> terrains)
            {
                Transform nodeContainer = GameObject.Find(Constants.NODE_CONTAINER_NAME).transform;

                Dictionary<Site, Location> siteToLoc = new Dictionary<Site, Location>();

                List<Location> locations = new List<Location>();

                for (int i = 0; i < sites.Count(); i++)
                {
                    Site site = sites[i];
                    Vector2 siteCoord = new Vector2(site.x, site.y);
                    Terrain terrain = null;
                    foreach (Terrain candidate in terrains)
                    {
                        Vector2[] vertices = candidate.plainMesh.vertices
                            .Select(vert => new Vector2(vert.x, vert.y))
                            .ToArray();
                        if (ContainsPoint(vertices, new Vector2(site.x, site.y)))
                        {
                            terrain = candidate;
                            break;
                        }
                    }
                    if (terrain == null)
                    {
                        Debug.Log("Site was outside terrain mesh");
                        continue;
                    }

                    Dictionary<Scenario, float> dict = terrain.tconfig.weightedScenarios;
                    Scenario scenario = WeightedRandomizer.From(dict).TakeOne();

                    Location location = new Location(i, terrain.tconfig.name, scenario, site.Coord);

                    siteToLoc[site] = location;

                    locations.Add(location);
                }

                foreach(Site site in sites)
                {
                    List<Site> neighbours = site.NeighborSites();
                    List<int> neighbourSiteIds = new List<int>();
                    Location siteLoc = siteToLoc[site];
                    
                    neighbourSiteIds = neighbours
                        .Select(neighbour => siteToLoc[neighbour].id) 
                        .ToList();

                    siteLoc.neighbours = neighbourSiteIds;
                }

                return locations;
            }

            /// <summary>
            /// Determines if the given point is inside the polygon
            /// </summary>
            /// <param name="polyPoints"></param>
            /// <param name="p"></param>
            /// <returns></returns>
            private bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
            {
                var j = polyPoints.Length - 1;
                var inside = false;
                for (int i = 0; i < polyPoints.Length; j = i++)
                {
                    var pi = polyPoints[i];
                    var pj = polyPoints[j];
                    if (((pi.y <= p.y && p.y < pj.y) || (pj.y <= p.y && p.y < pi.y)) &&
                        (p.x < (pj.x - pi.x) * (p.y - pi.y) / (pj.y - pi.y) + pi.x))
                        inside = !inside;
                }
                return inside;
            }
        }

        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;

        public GameObject mapUnderlay;
        public GameObject nodePrefab;
        public GameObject linePrefab;
        public Transform nodeContainer;
        public Transform edgeContainer;

        private Map map;

        private List<Site> sites;

        public GameObject guard;

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

            gem.StartListening("RegenerateScene", RegenerateMap);
            gem.StartListening("StartNewGame", GenerateMap);

            gem.StartListening("MouseEnterNode", ShowEdges);
            gem.StartListening("MouseExitNode", HideEdges);
            gem.StartListening("Arrived", StartScenario);
        }
        private void OnDestroy()
        {
            gem.StopListening("RegenerateScene", RegenerateMap);
            gem.StopListening("StartNewGame", GenerateMap);

            gem.StopListening("MouseEnterNode", ShowEdges);
            gem.StopListening("MouseExitNode", HideEdges);
            gem.StopListening("Arrived", StartScenario);
        }
        public void _GenerateMap()
        {
            GenerateMap();
        }
        public void _DestroyMap()
        {
            foreach (Location location in map.locations)
            {
                Destroy(location.node);
            }
            foreach (Terrain terrain in map.terrains)
            {
                Destroy(terrain.gameObject);
            }
            map = null;
        }
        public void _RegenerateMap()
        {
            RegenerateMap();
        }
        private void RegenerateMap()
        {
            map = gdm.GetGameData<Map>("Map");

            map.InstantiateMap(Instantiate);

            gem.TriggerEvent("RegeneratedMap", gameObject);

            gdm.Save();
        }
        private void GenerateMap()
        {
            float miny = mapUnderlay.GetComponent<MeshCollider>().bounds.min.y;
            float maxy = mapUnderlay.GetComponent<MeshCollider>().bounds.max.y;
            float minx = mapUnderlay.GetComponent<MeshCollider>().bounds.min.x;
            float maxx = mapUnderlay.GetComponent<MeshCollider>().bounds.max.x;

            sites = new Voronoi(
                CreateRandomPoint(miny, maxy, minx, maxx), 
                new Rectf(minx, miny, maxx - minx, maxy - miny), 
                2
            ).SitesIndexedByLocation.Values.ToList();
            List<Terrain> terrains = new TerrainGenerator().GenerateTerrains(terrainConfigScriptableObjects, miny, maxy, minx, maxx);
            List<Location> locations = new LocationsGenerator().GenerateLocations(sites, terrains);
            map = new MapGenerator().GenerateMap(locations, terrains);
            new MapFineTuner().PlaceWinScenarios(map, winScenario.GetPlainClass());
            map.InstantiateMap(Instantiate);

            gem.TriggerEvent("GeneratedMap", gameObject, new List<object> { map.locations });

            gdm.SetGameData<Map>("Map", map);
        }
        private void StartScenario(GameObject node, List<object> parameters, int x, int y, int tx, int ty)
        {
            Scenario scenario = map.GetScenario(node);
            gem.TriggerEvent("StartScenario", gameObject, new List<object> { scenario });
        }
        public bool CheckIfLocationAndNodeAreNeighbours(Location fromLocation, GameObject node)
        {
            Location toLocation = map.GetLocation(node);

            List<Location> neighbours = map.GetNeighboursOf(toLocation);

            foreach (Location neighbour in neighbours)
            {
                if (neighbour.id == fromLocation.id)
                {
                    return true;
                }
            }
            return false;
        }
        public Location GetLocation(GameObject node)
        {
            return map.GetLocation(node);
        }
        private List<GameObject> tempLines = new List<GameObject>();
        private void ShowEdges(GameObject invoker, List<object> parameters)
        {
            Location location = map.GetLocation(invoker);
            List<Location> neighbours = map.GetNeighboursOf(location);

            foreach (Location neighbour in neighbours)
            {
                Vector2f sitePos = location.position;
                Vector2f currPos = neighbour.position;
                tempLines.Add(CreateLine(sitePos, currPos, 0.25f));
            }
        }
        private void HideEdges(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            foreach (GameObject line in tempLines)
            {
                Destroy(line);
            }
            tempLines.Clear();
        }
        private GameObject CreateLine(Vector2f pos1, Vector2f pos2, float width)
        {
            GameObject lineObject = Instantiate(linePrefab, edgeContainer);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, new Vector3(pos1.x, pos1.y));
            lineRenderer.SetPosition(1, new Vector3(pos2.x, pos2.y));
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;

            return lineObject;
        }
        private List<Vector2f> CreateRandomPoint(float miny, float maxy, float minx, float maxx)
        {
            List<Vector2f> points = new List<Vector2f>();
            for (int i = 0; i < Constants.VORONOI_POLYGON_NUMBER; i++)
            {
                points.Add(new Vector2f(UnityEngine.Random.Range(minx, maxx), UnityEngine.Random.Range(miny, maxy)));
            }
                
            return points;
        }
    }
}
