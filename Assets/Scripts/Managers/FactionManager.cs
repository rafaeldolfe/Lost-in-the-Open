using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactionManager : MonoBehaviour
{
    private readonly Dictionary<string, List<GameObject>> factions = new Dictionary<string, List<GameObject>>();

    public void AddMember(string faction, GameObject gameObject)
    {
        if (factions.ContainsKey(faction))
        {
            factions[faction].Add(gameObject);
        }
        else
        {
            factions.Add(faction, new List<GameObject> { gameObject });
        }
    }
    public void RemoveMember(string faction, GameObject gameObject)
    {

        if (factions.ContainsKey(faction))
        {
            factions[faction].Remove(gameObject);
        }
    }
    public void AddMember(GameObject go)
    {
        if (go.GetComponent<Faction>() == null)
        {
            throw new System.Exception("Expected faction component, but found none");
        }
        string faction = go.GetComponent<Faction>().faction;

        AddMember(faction, go);
    }
    public void RemoveMember(GameObject go)
    {
        if (go.GetComponent<Faction>() == null)
        {
            throw new System.Exception("Expected faction component, but found none");
        }
        string faction = go.GetComponent<Faction>().faction;

        RemoveMember(faction, go);
    }
    public List<GameObject> GetFaction(string faction)
    {
        return factions[faction];
    }
    public List<List<GameObject>> GetAllMembersOfFactions()
    {
        return new List<List<GameObject>>(factions.Values);
    }
}
