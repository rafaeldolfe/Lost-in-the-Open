using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;
using System;
using System.Linq;
using WorldMap;
using Encounter;
using Sirenix.OdinInspector;

public class GlobalDebugManager : MonoBehaviour
{
    public GlobalEventManager gem;
    public GlobalPersistentDataManager gdm;
    public WorldMapManager wmm;
    public WorldMapPlayerManager wpm;
    public EncounterManager enm;
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
}
