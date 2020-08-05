using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class AbilityBarManager : MonoBehaviour
    {
        private GlobalEventManager gem;

        public List<Image> abilityFrames;
        public List<Image> abilityImages;
        public List<GameObject> abilityHighlights;

        private List<Ability> abilities;

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

            ResetAbilityBar();

            gem.StartListening("SetAbilityBar", SetAbilityBar);
            gem.StartListening("ResetAbilityBar", ResetAbilityBar);
            gem.StartListening("HighlightAbility", HighlightAbility);
        }

        void OnDestroy()
        {
            gem.StopListening("SetAbilityBar", SetAbilityBar);
            gem.StopListening("ResetAbilityBar", ResetAbilityBar);
            gem.StopListening("HighlightAbility", HighlightAbility);
        }
        private void SetAbilityBar(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            foreach (object ability in parameters)
            {
                if (!ability.GetType().IsSubclassOf(typeof(Ability)))
                {
                    throw new System.Exception("Expected list of abilities, found something of type " + ability.GetType());
                }
            }
            if (parameters.Count > Constants.SIZE_OF_ABILITY_BAR)
            {
                throw new System.Exception("Expected fewer abilities in parameters than " + Constants.SIZE_OF_ABILITY_BAR + ", found " + parameters.Count);
            }
            ResetAbilityBar();
            ShowAbilityBar();
            abilities = parameters.Select(p => (Ability)p).ToList();
            for (int i = 0; i < abilities.Count; i++)
            {
                Image sRenderer = abilityImages[i];
                sRenderer.sprite = abilities[i].GetSprite();
                sRenderer.color = Color.white;
            }
        }
        private void HideAbilityBar()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
                foreach (Transform secondchild in child.transform)
                {
                    secondchild.gameObject.SetActive(false);
                }
            }
        }
        private void ShowAbilityBar()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
                foreach (Transform secondchild in child.transform)
                {
                    secondchild.gameObject.SetActive(true);
                }
            }
        }
        private void ResetAbilityBar()
        {
            foreach (Image sRend in abilityImages)
            {
                sRend.sprite = null;
                sRend.color = new Color(0, 0, 0, 0);
            }
            foreach (GameObject go in abilityHighlights)
            {
                go.SetActive(false);
            }
            HideAbilityBar();
        }
        private void ResetHighlight()
        {
            foreach (GameObject abilityHighlight in abilityHighlights)
            {
                abilityHighlight.SetActive(false);
            }
        }
        private void HighlightAbility(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (gameObject)
                if (parameters.Count == 0)
                {
                    throw new System.Exception("Expected list with 1 ability, found empty list");
                }
            if (parameters[0] == null)
            {
                ResetHighlight();
                return;
            }
            if (!parameters[0].GetType().IsSubclassOf(typeof(Ability)))
            {
                throw new System.Exception("Expected subclass of type Ability, found element of type " + parameters[0].GetType());
            }
            Ability ability = (Ability)parameters[0];

            for (int i = 0; i < abilities.Count; i++)
            {
                if (ability == abilities[i])
                {
                    HighlightAbilitySlot(i);
                    return;
                }
            }
        }
        public void HighlightAbilitySlot(int index)
        {
            foreach (GameObject abilityHighlight in abilityHighlights)
            {
                if (abilityHighlight.active)
                {
                    abilityHighlight.SetActive(false);
                }
            }
            abilityHighlights[index].SetActive(true);
        }
        public GameObject GetAttachable()
        {
            return gameObject;
        }
    }
}