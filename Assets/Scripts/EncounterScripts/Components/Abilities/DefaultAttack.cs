using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;
using Sirenix.OdinInspector;

namespace Encounter
{
    [RequireComponent(typeof(AbilitiesHandler))]
    [RequireComponent(typeof(Position))]
    public class DefaultAttack : OffensiveAbility
    {
        private GlobalEventManager gem;
        private Pathfinding pf;
        private AbilitiesHandler ah;
        private Position pos;

        public int defaultAttackDamage;
        [OnValueChangedAttribute("UpdateRemainingAttacks")]
        public int numberOfAttacks;
        [SerializeField]
        private int remainingAttacks;
        private void UpdateRemainingAttacks()
        {
            remainingAttacks = numberOfAttacks;
        }
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (ah = gameObject.GetComponent<AbilitiesHandler>()),
            (pos = gameObject.GetComponent<Position>())
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            category = "Attack";
            highlightColor = new Color(0.75f, 0.29f, 0.22f, 0.78f); // Red
            pfconfig = new PathfindingConfig(ignoresTerrain: false, ignoresActors: true);

            gem.StartListening("Attack", RegisterAttackHandler);
        }

        public void OnDestroy()
        {
            gem.StopListening("Attack", RegisterAttackHandler);
        }
        public void RegisterAttackHandler(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (invoker != gameObject)
            {
                return;
            }
            remainingAttacks--;

            if (Done())
            {
                ah.AbilityDone();
            }
        }
        public override void UseAbility(List<PathNode> path)
        {
            if (Done())
            {
                throw new Exception("Tried to move without any remaining moves, or when the actor was not ready");
            }
            if (Status() == "Busy")
            {
                throw new Exception("Tried to move while actor was busy");
            }
            PathNode last = path.Last();

            gem.TriggerEvent("Attack", gameObject, new List<object> { defaultAttackDamage }, pos.x, pos.y, last.x, last.y);
        }
        public override int GetRange()
        {
            return range;
        }
        public override bool Done()
        {
            return remainingAttacks == 0;
        }
        public override string Status()
        {
            return "Idle"; // Always instantaneous attacks (not the case in the future)
        }
        public override void Reset(List<object> parameters)
        {
            remainingAttacks = numberOfAttacks;
        }
        public override float GetDamage()
        {
            return defaultAttackDamage;
        }
        public override List<PathNode> GetTargetsFrom(int x, int y)
        {
            return pf.FindPathNodesWithinRange(x, y, GetRange(), pfconfig);
        }
        public override List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty)
        {
            return pf.FindPath(x, y, tx, ty, pfconfig);
        }
    }
}