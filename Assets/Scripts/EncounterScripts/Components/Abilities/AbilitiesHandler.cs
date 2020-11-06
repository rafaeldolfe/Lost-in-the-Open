using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Utils;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.OdinInspector;

namespace Encounter
{
    public class AbilitiesHandler : MonoBehaviour
    {
        private GlobalEventManager gem;
        private Pathfinding pf;
        private EventLoopManager elm;
        private Position pos;
        private FactionManager fm;

        private List<ActiveAbility> activeAbilities;
        private List<PassiveAbility> passiveAbilities;

        private ActiveAbility current;

        private void CacheAbilities()
        {
            activeAbilities = GetComponents<MonoBehaviour>()
                .Where(mono => mono is ActiveAbility)
                .Select(ab => (ActiveAbility)ab)
                .ToList();
            passiveAbilities = GetComponents<MonoBehaviour>()
                .Where(mono => mono is PassiveAbility)
                .Select(ab => (PassiveAbility)ab)
                .ToList();
            if ((activeAbilities == null || activeAbilities.Count == 0) && (passiveAbilities == null || passiveAbilities.Count == 0))
            {
                throw new Exception("Expected Ability component(s) to be attached to gameObject, found none");
            }
        }
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
                (elm = FindObjectOfType(typeof(EventLoopManager)) as EventLoopManager),
                (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
                (pos = gameObject.GetComponent<Position>())
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            CacheAbilities();
            current = activeAbilities[0];
            gem.StartListening(fm.GetFactionOf(gameObject) + "EndTurn", ResetAbilities);
            gem.StartListening("CHEAT_EndTurn", ResetAbilities);
        }
        void OnDestroy()
        {
            gem.StopListening(fm.GetFactionOf(gameObject) + "EndTurn", ResetAbilities);
            gem.StopListening("CHEAT_EndTurn", ResetAbilities);

            elm.CancelEvents(activeAbilities
                .ConvertAll(ab => (Ability) ab)
                .Concat(passiveAbilities
                    .ConvertAll(ab => (Ability)ab))
                .ToList());
        }

        private void ResetAbilities()
        {
            foreach (Ability ability in activeAbilities)
            {
                ability.Reset();
            }
            foreach (Ability ability in passiveAbilities)
            {
                ability.Reset();
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
            foreach (ActiveAbility ability in activeAbilities)
            {
                if (!ability.Done())
                {
                    return true;
                }
            }
            return false;
        }
        public List<GridContainer> GetTargetTiles(int tx, int ty)
        {
            if (current == null)
            {
                return new List<GridContainer>();
            }
            return current.GetTargetTiles(tx, ty).ConvertAll(p => p.parent);
        }

        public string GetStatus()
        {
            foreach (Ability ability in GetAbilities())
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
            foreach (ActiveAbility ability in activeAbilities)
            {
                if (!ability.Done())
                {
                    SetAbility(ability);
                    return;
                }
            }
            SetAbility(default(ActiveAbility));
        }
        public void SetAbility(int index)
        {
            if (index < activeAbilities.Count)
            {
                SetAbility(activeAbilities[index]);
            }
            else
            {
                SetAbility(default(ActiveAbility));
            }
        }
        private void SetAbility(ActiveAbility ability)
        {
            gem.TriggerEvent("HighlightAbility", gameObject, new List<object> { ability });
            current = ability;
        }
        public IEnumerator UseAbility(int tx, int ty)
        {
            if (IsCurrentAbilityDone())
            {
                yield break;
            }

            if (!current.IsTargetWithinRange(tx, ty))
            {
                yield break;
            }

            DecisionsEnumerator enu = new DecisionsEnumerator(current.BreakDownAbility(tx, ty));
            yield return enu;
            foreach (Decision dec in enu.Result)
            {
                if (dec.ability.GetType() is IMovement)
                {
                    GlobalDebugManager.Instance.HighlightTiles(dec.path);
                }
                elm.AddEvent(dec);
            }
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
            return activeAbilities
                .ConvertAll(ab => (Ability)ab)
                .Concat(passiveAbilities
                    .ConvertAll(ab => (Ability)ab))
                .ToList();
        }
        public List<PathNode> GetTilesWithinRange()
        {
            return current.GetTilesWithinRange();
        }

        internal List<ActiveAbility> GetActiveAbilities()
        {
            return activeAbilities;
        }
    }
}