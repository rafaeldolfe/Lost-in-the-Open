using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{

    [RequireComponent(typeof(Position))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Faction))]
    [RequireComponent(typeof(AbilitiesHandler))]
    public class Actor : MonoBehaviour
    {
        public string name;
        [SerializeField]
        private Sprite portrait;

        private GlobalEventManager gem;
        private Position pos;
        private Health hp;
        private Faction fac;
        private AbilitiesHandler ah;

        void Start()
        {
            gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager;
            pos = GetComponent<Position>();
            hp = GetComponent<Health>();
            fac = GetComponent<Faction>();
            ah = GetComponent<AbilitiesHandler>();
            if (gem == null || pos == null || hp == null || fac == null || ah == null)
            {
                List<MonoBehaviour> deps = new List<MonoBehaviour> { gem, pos, hp, fac, ah };
                List<Type> depTypes = new List<Type> { typeof(GlobalEventManager), typeof(Position), typeof(Health), typeof(Faction), typeof(AbilitiesHandler) };
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("Death", Death);
        }

        public void OnDestroy()
        {
            gem.StopListening("Death", Death);
        }

        public void Death(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (invoker != gameObject)
            {
                return;
            }
            // implement death animation thing
            Destroy(invoker);
        }
        public Sprite GetPortrait()
        {
            return portrait;
        }
    }
}