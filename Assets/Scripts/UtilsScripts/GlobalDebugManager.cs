using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using System.Linq;
using WorldMap;
using Encounter;
using Sirenix.OdinInspector;
using TMPro;

public class GlobalDebugManager : MonoBehaviour
{
    public static GlobalDebugManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GlobalDebugManager>();
            }
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
    private static GlobalDebugManager _instance;

    public GlobalEventManager gem;
    public GlobalPersistentDataManager gdm;
    public WorldMapManager wmm;
    public WorldMapPlayerManager wpm;
    public EncounterManager enm;
    public Pathfinding pf;
    public DomainKnowledgeManager dkm;
    public AIManager aim;
    public MapManager mgm;
    public FactionManager fm;

    public static Color DEBUG_HIGHLIGHT_COLOR = new Color(0.67f, 0.84f, 0.90f);
    public static bool debug = true;
    private List<GameObject> _ListOfTexts = new List<GameObject>();

    public GameObject tileHighlight;
    [Title("Utilities")]
    [Button]
    public void GetDependencies()
    {
        List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
            (wmm = FindObjectOfType(typeof(WorldMapManager)) as WorldMapManager),
            (wpm = FindObjectOfType(typeof(WorldMapPlayerManager)) as WorldMapPlayerManager),
            (enm = FindObjectOfType(typeof(EncounterManager)) as EncounterManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (dkm = FindObjectOfType(typeof(DomainKnowledgeManager)) as DomainKnowledgeManager),
            (aim = FindObjectOfType(typeof(AIManager)) as AIManager),
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
        };
    }
    [Title("Encounter")]
    [Button]
    public void DestroyAllEnemies()
    {
        enm._GetEnemyUnits().ForEach(Destroy);
    }
    [Button]
    public void GetAllPlayerPositions()
    {
        enm._GetPlayerUnits().ForEach(e => { 
            Debug.Log(e.GetComponent<Position>().GetX());
            Debug.Log(e.GetComponent<Position>().GetY());
        });
    }
    [Title("WorldMap")]
    [Button]
    public void ResetCache()
    {
        gdm._ResetCache();
    }
    [Button]
    public void CheckCache()
    {
        gdm._LogCache();
    }
    [Button]
    public void Load()
    {
        gdm.LoadSave(0);
    }
    [Button]
    public void Save()
    {
        gdm.Save(0);
    }
    [Button]
    public void GenerateMap()
    {
        wmm._GenerateMap();
    }
    [Button]
    public void DestroyMap()
    {
        wpm._DestroyPlayer();
        wmm._DestroyMap();
    }
    [Button]
    public void RegenerateMap()
    {
        wmm._RegenerateMap();
    }
    [Button]
    public void GameOver()
    {
        gem.TriggerEvent("GameOver");
    }

    Task debugTask;
    private void Update()
    {
        if (Input.GetKeyDown("m"))
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            if (_ListOfTexts.Count == 0)
            {
                foreach (GridContainer gc in mgm.grid.tiles)
                {
                    if (gc.pn.isWalkable)
                    {
                        GameObject go = new GameObject();
                        go.AddComponent<TextMeshPro>();
                        TextMeshPro tmp = go.GetComponent<TextMeshPro>();
                        tmp.text = $"{gc.x} , {gc.y}";
                        tmp.fontSize = 2;
                        go.transform.position = new Vector3(gc.x, gc.y, 0);
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.alignment = TextAlignmentOptions.Midline;
                        _ListOfTexts.Add(go);
                    }
                }
            }
            else
            {
                _ListOfTexts.ForEach(a => Destroy(a));
                _ListOfTexts.Clear();
            }
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (mgm = FindObjectOfType(typeof(MapManager)) as MapManager),
                (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }

            int x = ProgramUtils.GetMouseGridPosition().x;
            int y = ProgramUtils.GetMouseGridPosition().y;
            GridContainer current = mgm.grid.GetGridObject(x, y);

            if (current != null && current.actor != null && fm.GetFactionOf(current.actor) == "Enemy" && (debugTask == null || debugTask.Running != true))
            {
                ResetText();
                debugTask = Task.Get(GlobalDebugManager.Instance.DisplayTileEvaluationsAnalysis(x, y, current.actor), true);
            }
        }
    }
    private void ResetText()
    {
        UnityEngine.Object.Destroy(GameObject.Find("Debug"));
    }
    public IEnumerator DisplayTileEvaluationsAnalysis(int x, int y, GameObject actor)
    {
        pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding;
        dkm = FindObjectOfType(typeof(DomainKnowledgeManager)) as DomainKnowledgeManager;
        aim = FindObjectOfType(typeof(AIManager)) as AIManager;

        pf.ResetCaches();

        yield return StartCoroutine(dkm.GenerateTileEvaluations(x, y));

        List<(List<Decision>, float)> evaluation = aim._GetEvaluations(actor);

        foreach ((List<Decision>, float) eval in evaluation)
        {
            foreach (Decision decision in eval.Item1)
            {
                if (decision.ability is IMovement)
                {
                    PathNode finalPos = decision.path.Last();

                    if (GameObject.Find("Debug") == null)
                    {
                        new GameObject("Debug");
                    }
                    GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
                    RectTransform transform = gameObject.GetComponent<RectTransform>();
                    transform.SetParent(GameObject.Find("Debug").transform);
                    transform.Rotate(new Vector3(0, 0, 0));
                    transform.localPosition = new Vector3(finalPos.x, finalPos.y, 1);// localPosition;
                    transform.sizeDelta = new Vector2(1, 1);
                    TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
                    textMesh.alignment = TextAlignmentOptions.Center;
                    textMesh.text = string.Format("{0}", Math.Round(eval.Item2, 2).ToString());
                    textMesh.fontSize = 3.5f;

                    textMesh.color = Color.black;
                    textMesh.GetComponent<MeshRenderer>().sortingOrder = 5000;
                }
            }
        }
    }

    public void HighlightTiles(List<PathNode> tiles)
    {
        if (tiles == null || tiles.Count == 0)
        {
            return;
        }
        List<(GridContainer, GameObject)> highlights = new List<(GridContainer, GameObject)>();
        List<GridContainer> gcs = tiles.Select(p => p.parent).ToList();
        foreach (GridContainer gc in gcs)
        {
            GameObject tileHighlightInstance = Instantiate(tileHighlight, tileHighlight.transform.position + new Vector3(gc.x, gc.y, 0), tileHighlight.transform.rotation);
            tileHighlightInstance.GetComponent<Renderer>().GetComponent<Renderer>().material.color = DEBUG_HIGHLIGHT_COLOR;
            gc.AddGameObject(tileHighlightInstance);
            highlights.Add((gc, tileHighlightInstance));
        }
        StartCoroutine(SlowFadeHighlight(highlights));
    }
    private IEnumerator SlowFadeHighlight(List<(GridContainer, GameObject)> tiles)
    {
        while (true)
        {
            tiles.ForEach(pair => {
                Color c = pair.Item2.GetComponent<Renderer>().material.color;
                Color fadedColor = new Color(c.r, c.g, c.b, c.a - Time.deltaTime);
                pair.Item2.GetComponent<Renderer>().material.color = fadedColor;
            });
            if (tiles[0].Item2.GetComponent<Renderer>().material.color.a <= 0)
            {
                tiles.ForEach(pair => {
                    pair.Item1.RemoveGameObject(pair.Item2);
                    Destroy(pair.Item2);
                });
                break;
            }
            yield return null;
        }
    }

    public static void PrintList<T>(List<T> l)
    {
        foreach (T item in l)
        {
            Debug.Log(item);
        }
    }
    public static void PrintDecision(Decision decision)
    {
        if (decision == null)
        {
            throw new Exception("Missing decision object: Found null");
        }
        if (decision.ability == null)
        {
            throw new Exception("Missing ability object: Found null");
        }
        if (decision.path == null)
        {
            throw new Exception("Missing list of path nodes: Found null");
        }
        if (decision.path.Count == 0)
        {
            throw new Exception("Missing path nodes: Found empty list");
        }
        Debug.Log("Ability: " + decision.ability.GetType());
        Debug.Log(String.Format("Path: ({0},{1}) to ({2},{3})",
            decision.path.First().x, decision.path.First().y,
            decision.path.Last().x, decision.path.Last().y));
    }
    public static void PrintPathNode(PathNode pathNode)
    {
        if (pathNode == null)
        {
            throw new Exception("Missing decision object: Found null");
        }
        Debug.Log(String.Format("PathNode: ({0},{1})", pathNode.x, pathNode.y));
    }
    public static void HighlightPathNodes(List<PathNode> pathNodes, Color color = default)
    {
        color = color != default ? color : Color.white;
        foreach (PathNode pathNode in pathNodes)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.GetComponent<MeshRenderer>().material.color = color;
            cube.transform.position = new Vector3(pathNode.x, pathNode.y, 1);
            cube.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            //cube.transform.SetParent(GameObject.Find("Debug").transform);
        }
    }
}
