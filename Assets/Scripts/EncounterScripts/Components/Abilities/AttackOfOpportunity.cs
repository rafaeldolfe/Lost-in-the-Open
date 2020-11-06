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
    public class AttackOfOpportunity : PassiveAbility, IOffensive
    {
        private GlobalEventManager gem;
        private Pathfinding pf; 
        private AbilitiesHandler ah;
        private EventLoopManager elm;
        private FactionManager fm;
        private Position pos;

        public int opportunityDamage;
        private List<PathNode> opportunityTiles = new List<PathNode>();
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
                (elm = FindObjectOfType(typeof(EventLoopManager)) as EventLoopManager),
                (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
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

            gem.StartListening(fm.GetFactionOf(gameObject) + "EndTurn", RegisterAttackOfOpportunity);
        }

        public void OnDestroy()
        {
            gem.StopListening(fm.GetFactionOf(gameObject) + "EndTurn", RegisterAttackOfOpportunity);
            gem.StopListening("Move", ExecuteAttackOfOpportunity);
        }
        public void RegisterAttackOfOpportunity()
        {
            opportunityTiles = GetTilesWithinRange();

            gem.StartListening("Move", ExecuteAttackOfOpportunity);
        }
        private void ExecuteAttackOfOpportunity(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            PathNode opportunityTile = pf.GetNode(tx, ty);

            if (fm.GetFactionOf(opportunityTile.parent.actor) == fm.GetFactionOf(gameObject))
            {
                return;
            }

            if (!opportunityTiles.Contains(opportunityTile) || remainingAttacks == 0)
            {
                return;
            }

            elm.AddHardInterrupt(new Decision(this, new List<PathNode> { opportunityTile }));

            if (Done())
            {
                gem.StopListening("Move", ExecuteAttackOfOpportunity);
            }
        }
        private void Attacked()
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
            PathNode target = targets.Last();
            tilesWithinRange.Clear();
            gem.TriggerEvent("Attack", gameObject, new List<object> { opportunityDamage }, pos.x, pos.y, target.x, target.y);
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
            gem.StopListening("Move", ExecuteAttackOfOpportunity);
            remainingAttacks = numberOfAttacks;
        }
        public List<PathNode> GetTargetsFrom(int x, int y)
        {
            tilesWithinRange = pf.DijkstraWithinRangeCaching(this, x, y, GetRange(), pfconfig);
            tilesWithinRange.RemoveAt(0);
            return tilesWithinRange;
        }
        public List<PathNode> GetPathToTargetFrom(int x, int y, int tx, int ty)
        {
            return pf.FindPathWithinRange(this, x, y, tx, ty);
        }

        public override IEnumerator BreakDownAbility(int tx, int ty)
        {
            yield return new List<Decision> {
                new Decision(
                    this, new List<PathNode> { pf.GetNode(tx, ty) })
                };
        }

        public List<PathNode> GetTilesWithinRange()
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

        public List<PathNode> GetTargetTiles(int tx, int ty)
        {
            return new List<PathNode> { pf.GetNode(tx, ty) };
        }

        public float GetDamage()
        {
            return opportunityDamage;
        }
    }
}