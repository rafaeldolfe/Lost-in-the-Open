using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace WorldMap
{
    public class NodeClickScript : MonoBehaviour
    {
        private GlobalEventManager gem;

        private bool hovering;
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
        private void Update()
        {
            if (hovering && Input.GetMouseButtonDown(0))
            {
                Clicked();
            }
        }
        private void Clicked()
        {
            if (PauseService.IsLevelPaused(PauseService.TEXT_PAUSE))
            {
                return;
            }
            gem.TriggerEvent("ClickNode", gameObject);
        }
        void OnMouseEnter()
        {
            hovering = true;
        }
        void OnMouseExit()
        {
            hovering = false;
        }
    }
}
