using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Faction : MonoBehaviour
{
    private FactionManager fm;

    public string faction;

    void Awake()
    {
        fm = FindObjectOfType(typeof(FactionManager)) as FactionManager;
        if (fm == null)
        {
            List<MonoBehaviour> deps = new List<MonoBehaviour> { fm };
            List<Type> depTypes = new List<Type> { typeof(FactionManager) };
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        fm.AddMember(faction, gameObject);
    }

    void OnDestroy()
    {
        fm.RemoveMember(faction, gameObject);
    }

    public List<GameObject> GetFactionMembers()
    {
        return fm.GetFaction(faction);
    }
}
