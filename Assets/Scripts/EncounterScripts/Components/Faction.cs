using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    public class Faction : MonoBehaviour
    {
        private FactionManager fm;

        public string faction;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
            };
            if (deps.Contains(null))
            {
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
}