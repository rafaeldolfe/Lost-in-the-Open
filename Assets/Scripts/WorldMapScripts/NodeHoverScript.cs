using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace WorldMap
{
    public class NodeHoverScript : MonoBehaviour
    {
        private GlobalEventManager gem;

        private MeshRenderer meshr;
        private Color original;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
        }
        private void Start()
        {
            meshr = GetComponent<MeshRenderer>();

            original = meshr.material.color;
        }
        void OnMouseEnter()
        {
            meshr.material.color = original + Constants.NODE_HIGHLIGHT;
            gem.TriggerEvent("MouseEnterNode", gameObject);
        }
        void OnMouseExit()
        {
            meshr.material.color = original;
            gem.TriggerEvent("MouseExitNode", gameObject);
        }
    }
}
