using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
using UnityEngine;
using System.Linq;
using System;
using TMPro;
using Utils;


namespace Encounter
{
    [RequireComponent(typeof(Position))]
    [RequireComponent(typeof(BehaviourHandler))]
    [RequireComponent(typeof(AbilitiesHandler))]

    public class AttackMove : Behaviour
    {
        private DomainKnowledgeManager dkm;
        private Pathfinding pf;
        private AbilitiesHandler ah;
        private Position pos;

        public float vFactor = 1;
        public float enemiesFactor = 1;
        public float alliesFactor = 1;
        public float objectiveFactor = 1;

        private List<Func<PathNode, bool>> attackableConditions;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (dkm = FindObjectOfType(typeof(DomainKnowledgeManager)) as DomainKnowledgeManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (ah = gameObject.GetComponent<AbilitiesHandler>()),
            (pos = gameObject.GetComponent<Position>()),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            attackableConditions = new List<Func<PathNode, bool>>();
        }
        void Start()
        {
            foreach (Ability ability in ah.GetAbilities())
            {
                if (ability.category != "Movement")
                {
                    continue;
                }
            }
            attackableConditions.Add(IsPlayerActorCondition);
        }
        public override Analysis GetAnalysis(List<Ability> abilities)
        {

            //foreach(List<Decision> decisions in possibleCoursesOfAction)
            //{
            //    foreach(Decision decision in decisions)
            //    {
            //        ProgramUtils.PrintDecision(decision);
            //    }
            //}
            List<List<Decision>> possibleCoursesOfAction = GetPossibleCoursesOfAction(abilities);
            List<(List<Decision>, float)> evaluations = GetEvaluations(possibleCoursesOfAction);
            List<List<Decision>> filteredCoursesOfAction = evaluations.Take(5).Select(ev => ev.Item1).ToList();
            return new Analysis(this, filteredCoursesOfAction);
        }
        private List<List<Decision>> GetPossibleCoursesOfAction(List<Ability> abilities)
        {
            Profiler.BeginSample("GetAnalysis: initialize movement/attack abilities");
            List<ActiveAbility> movementAbilities = abilities
                .Where(ability => ability is IMovement && ability is ActiveAbility)
                .Select(movementAbility => (ActiveAbility)movementAbility)
                .ToList();
            List<ActiveAbility> attackAbilities = abilities
                .Where(ability => ability is IOffensive && ability is ActiveAbility)
                .Select(offensiveAbility => (ActiveAbility)offensiveAbility)
                .ToList();
            Profiler.EndSample();

            Profiler.BeginSample("GetAnalysis: foreach (MovementAbility movementAbility in movementAbilities)");
            List<List<Decision>> possibleCoursesOfAction = new List<List<Decision>>();
            foreach (ActiveAbility movementAbility in movementAbilities)
            {
                List<PathNode> movementPossibilities = movementAbility.GetTargetsFrom(pos.x, pos.y);
                foreach (PathNode tp in movementPossibilities)
                {
                    foreach (ActiveAbility attackAbility in attackAbilities)
                    {
                        List<PathNode> attackPossibilities = attackAbility.GetTargetsFrom(tp.x, tp.y);

                        List<PathNode> attackablePathNodes = attackPossibilities
                            .Where(p => attackableConditions.All(condition => condition(p)))
                            .ToList();
                        foreach (PathNode attackable in attackablePathNodes)
                        {
                            List<PathNode> movement = movementAbility.GetPathToTargetFrom(pos.x, pos.y, tp.x, tp.y);
                            List<PathNode> attack = attackAbility.GetPathToTargetFrom(tp.x, tp.y, attackable.x, attackable.y);
                            if (movement == null || attack == null)
                            {
                                Debug.Log($"We fkd up");
                            }
                            List<Decision> courseOfAction = new List<Decision> {
                            new Decision(movementAbility, movement),
                            new Decision(attackAbility, attack)
                        };

                            possibleCoursesOfAction.Add(courseOfAction);
                        }
                    }
                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("GetAnalysis: if (possibleCoursesOfAction.Count == 0)");
            if (possibleCoursesOfAction.Count == 0)
            {
                foreach (ActiveAbility movementAbility in movementAbilities)
                {
                    List<PathNode> movementPossibilities = movementAbility.GetTargetsFrom(pos.x, pos.y);
                    foreach (PathNode tp in movementPossibilities)
                    {
                        List<PathNode> finalPath = movementAbility.GetPathToTargetFrom(pos.x, pos.y, tp.x, tp.y);
                        Decision decision = new Decision(movementAbility, finalPath);
                        possibleCoursesOfAction.Add(new List<Decision> { decision });
                    }
                }
            }
            Profiler.EndSample();

            return possibleCoursesOfAction;
        }
        private List<(List<Decision>, float)> GetEvaluations(List<List<Decision>> decisions)
        {
            return SelectStrongestCoursesOfAction(decisions);
        }
        public override List<(List<Decision>, float)> _GetEvaluations()
        {
            List<List<Decision>> possibleCoursesOfAction = GetPossibleCoursesOfAction(ah.GetAbilities());
            return GetEvaluations(possibleCoursesOfAction);
        }
        private List<(List<Decision>, float)> SelectStrongestCoursesOfAction(List<List<Decision>> coursesOfAction)
        {
            Profiler.BeginSample("GetAnalysis: SelectStrongestCoursesOfAction");
            List<(List<Decision>, float)> evaluation = coursesOfAction.Select(course => (course, EvaluateStrength(course))).ToList();
            Profiler.BeginSample("GetAnalysis: evaluation.Sort");
            evaluation.Sort((course1, course2) => -course1.Item2.CompareTo(course2.Item2));
            Profiler.EndSample();
            Profiler.EndSample();

            return evaluation.ToList();
        }
        private float EvaluateStrength(List<Decision> courseOfAction)
        {
            float movementStrength = 0;
            float damageStrength = 0;
            foreach (Decision decision in courseOfAction)
            {
                if (decision.ability is IMovement)
                {
                    Profiler.BeginSample("GetAnalysis: EvaluateMovement");
                    movementStrength = EvaluateMovement(decision);
                    Profiler.EndSample();
                }
                else if (decision.ability is IOffensive)
                {
                    Profiler.BeginSample("GetAnalysis: EvaluateAttack");
                    damageStrength = EvaluateAttack(decision);
                    Profiler.EndSample();
                }
            }
            return movementStrength + damageStrength;
        }
        private float EvaluateMovement(Decision decision)
        {
            PathNode finalPosition = decision.path.Last();

            float accumulativeStrength = dkm.EvaluatePositionalStrength(finalPosition, vFactor, enemiesFactor, alliesFactor, objectiveFactor);

            if (accumulativeStrength < 0.01f)
            {

            }

            return accumulativeStrength;
        }
        private float EvaluateAttack(Decision decision)
        {
            PathNode finalPosition = decision.path.Last();
            if (finalPosition.hasActor)
            {
                return (decision.ability as IOffensive).GetDamage();
            }
            return 0;
        }
        private bool IsPlayerActorCondition(PathNode attackablePathNode)
        {
            return pf.DoesPathNodeHavePlayerActor(attackablePathNode);
        }
    }
}