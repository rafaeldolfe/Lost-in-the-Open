using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace WorldMap
{
    public class OptionInputListenerScript : MonoBehaviour
    {
        private GlobalEventManager gem;

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
            if (Input.GetKeyDown($"{GetOptionIndex() + 1}"))
            {
                Clicked();
            }
        }

        public void Clicked()
        {
            if (PauseService.IsLevelPaused(PauseService.MENU_PAUSE))
            {
                return;
            }
            gem.TriggerEvent("OptionClicked", gameObject, new List<object> { GetOptionIndex() });
        }

        private int GetOptionIndex()
        {
            int counter = 0;
            foreach (Transform child in transform.parent.transform)
            {
                if (child == transform)
                {
                    return counter;
                }
                else if (child.CompareTag("Option"))
                {
                    counter++;
                }
            }
            throw new Exception($"Something went awfully wrong. Parent did not contain child at all. Parent has this many elements: {transform.parent.transform.childCount}");
        }
    }
}
