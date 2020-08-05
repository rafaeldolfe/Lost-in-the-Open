using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    public class AbilitiesHandler : MonoBehaviour
    {
        private GlobalEventManager gem;
        private Pathfinding pf;
        private Position pos;

        public List<string> abilityNames;
        private List<Ability> abilities;

        private Ability current;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (pos = gameObject.GetComponent<Position>())
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            CacheAbilities();
            if (abilities == null || abilities.Count == 0)
            {
                throw new Exception("Expected Ability component(s) to be attached to gameObject, found none");
            }
            current = abilities[0];
            gem.StartListening("EndTurn", ResetAbilities);
            gem.StartListening("CHEAT_EndTurn", ResetAbilities);
        }
        void OnDestroy()
        {
            gem.StopListening("EndTurn", ResetAbilities);
            gem.StopListening("CHEAT_EndTurn", ResetAbilities);
        }

        private void ResetAbilities(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            foreach (Ability ability in abilities)
            {
                ability.Reset(parameters);
            }
        }
        public void AbilityDone()
        {
            SetFirstAvailableAbility();
            if (!IsAnyAbilityLeft())
            {
                gem.TriggerEvent("ActorIsDone", gameObject);
                return;
            }
        }
        public bool IsCurrentAbilityDone()
        {
            if (current == null)
            {
                return true;
            }
            return current.Done();
        }
        public bool IsAnyAbilityLeft()
        {
            foreach (Ability ability in abilities)
            {
                if (!ability.Done())
                {
                    return true;
                }
            }
            return false;
        }
        public string GetStatus()
        {
            foreach (Ability ability in abilities)
            {
                if (ability.Status() == "Busy")
                {
                    return "Busy";
                }
            }
            return "Idle";
        }
        public void SetFirstAvailableAbility()
        {
            foreach (Ability ability in abilities)
            {
                if (!ability.Done())
                {
                    SetAbility(ability);
                    return;
                }
            }
            SetAbility(default(Ability));
        }
        public void SetAbility(string abilityName)
        {
            foreach (Ability ability in abilities)
            {
                if (abilityName == ability.GetAbilityName())
                {
                    SetAbility(ability);
                    return;
                }
            }
            SetAbility(default(Ability));
        }
        public void SetAbility(int index)
        {
            if (index < abilities.Count)
            {
                SetAbility(abilities[index]);
            }
            else
            {
                SetAbility(default(Ability));
            }
        }
        private void SetAbility(Ability ability)
        {
            gem.TriggerEvent("HighlightAbility", gameObject, new List<object> { ability });
            current = ability;
        }
        public void UseAbility(int tx, int ty)
        {
            if (IsCurrentAbilityDone())
            {
                return;
            }
            List<PathNode> path = GetPathWithinRange(tx, ty);
            current.UseAbility(path);
        }
        public Color GetHighlightColor()
        {
            if (current == null)
            {
                throw new Exception("Cannot get highlight color from null");
            }
            return current.highlightColor;
        }
        public List<Ability> GetAbilities()
        {
            return abilities;
        }
        public List<PathNode> GetTilesWithinRange()
        {
            return pf.FindPathNodesWithinRange(pos.x, pos.y, current.GetRange(), current.pfconfig);
        }
        public List<PathNode> GetPathWithinRange(int tx, int ty)
        {
            if (pos.x == tx && pos.y == ty)
            {
                throw new Exception("Cannot use ability on self");
            }
            List<PathNode> path = pf.FindPathInto2(pos.x, pos.y, tx, ty, current.pfconfig);
            if (path.Count - 1 <= current.GetRange())
            {
                return path;
            }
            else if (ProgramDebug.debug)
            {
                Debug.Log("Unable to find path from (" + pos.x + "," + pos.y + ") to (" + tx + "," + ty + ")");
            }
            return null;
        }
        private void CacheAbilities()
        {
            abilities = abilityNames.ConvertAll<Ability>(ab => (Ability)gameObject.GetComponent(ab));
        }
    }
}