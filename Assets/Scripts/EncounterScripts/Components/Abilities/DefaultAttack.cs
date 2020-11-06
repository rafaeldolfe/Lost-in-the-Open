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
    public class DefaultAttack : ActiveAbility, IOffensive
    {
        private GlobalEventManager gem;
        private Pathfinding pf;
        private AbilitiesHandler ah;
        private Position pos;

        public int defaultAttackDamage;
        [OnValueChangedAttribute("UpdateRemainingAttacks")]
        public int numberOfAttacks;
        [SerializeField]
        [HideInInspector]
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
                (ah = GetComponent<AbilitiesHandler>()),
                (pos = GetComponent<Position>())
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            category = "Attack";
            highlightColor = new Color(0.75f, 0.29f, 0.22f, 0.78f); // Red
            pfconfig = new PathfindingConfig(ignoreAll: true, ignoreLastTile: true, ignoreActors: true);
        }
        public void Attacked()
        {
            remainingAttacks--;

            if (Done())
            {
                ah.AbilityDone();
            }
        }
        public override IEnumerator UseAbility(List<PathNode> targets)
        {
            if (Done())
            {
                throw new Exception("Tried to move without any remaining moves, or when the actor was not ready");
            }
            if (Status() == "Busy")
            {
                throw new Exception("Tried to move while actor was busy");
            }

            tilesWithinRange.Clear();
            gem.TriggerEvent("Attack", gameObject, new List<object> { defaultAttackDamage }, pos.x, pos.y, targets.Last().x, targets.Last().y);
            Attacked();
            yield return null;
        }
        public override float GetRange()
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
        public override void Reset()
        {
            remainingAttacks = numberOfAttacks;
        }
        public override List<PathNode> GetTargetsFrom(int x, int y)
        {
            tilesWithinRange = pf.DijkstraWithinRangeCaching(this, x, y, GetRange(), pfconfig);
            tilesWithinRange.RemoveAt(0);
            return tilesWithinRange;
        }
        public override List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty)
        {
            return pf.FindPathWithinRange(this, x, y, tx, ty);
        }

        public override IEnumerator BreakDownAbility(int tx, int ty)
        {
            yield return new List<Decision> {
                new Decision(
                    this, new List<PathNode> { pf.GetNode(tx, ty) }
                    )
                };
        }

        public override List<PathNode> GetTilesWithinRange()
        {
            return GetTilesWithinRange(pos.x, pos.y);
        }
        private List<PathNode> GetTilesWithinRange(int x, int y)
        {
            tilesWithinRange = pf.DijkstraWithinRange(x, y, GetRange(), pfconfig);
            tilesWithinRange.RemoveAt(0);
            return tilesWithinRange;
        }

        public override List<Decision> BreakDownAbility(List<PathNode> path)
        {
            return new List<Decision> { new Decision(this, new List<PathNode> { pf.GetNode(path.Last().x, path.Last().y) }) };
        }

        public override List<PathNode> GetTargetTiles(int tx, int ty)
        {
            return new List<PathNode> { pf.GetNode(tx, ty) };
        }
        public float GetDamage()
        {
            return defaultAttackDamage;
        }
    }
}