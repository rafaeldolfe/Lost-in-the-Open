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
            List<int> ranges = new List<int>();
            foreach (Ability ability in ah.GetAbilities())
            {
                if (ability.category != "Movement")
                {
                    continue;
                }
                ranges.Add(ability.range);
            }
            attackableConditions.Add(IsPlayerActorCondition);
        }
        public override Analysis GetAnalysis(List<Ability> abilities)
        {
            Profiler.BeginSample("GetAnalysis: initialize movement/attack abilities");
            List<MovementAbility> movementAbilities = abilities
                .Where(ability => ability.GetType().IsSubclassOf(typeof(MovementAbility)))
                .Select(movementAbility => (MovementAbility)movementAbility)
                .ToList();
            List<OffensiveAbility> attackAbilities = abilities
                .Where(ability => ability.GetType().IsSubclassOf(typeof(OffensiveAbility)))
                .Select(offensiveAbility => (OffensiveAbility)offensiveAbility)
                .ToList();
            Profiler.EndSample();

            Profiler.BeginSample("GetAnalysis: foreach (MovementAbility movementAbility in movementAbilities)");
            List<List<Decision>> possibleCoursesOfAction = new List<List<Decision>>();
            foreach (MovementAbility movementAbility in movementAbilities)
            {
                List<PathNode> movementPossibilities = movementAbility.GetTargetsFrom(pos.x, pos.y);
                foreach (PathNode tp in movementPossibilities)
                {
                    foreach (OffensiveAbility attackAbility in attackAbilities)
                    {
                        List<PathNode> attackPossibilities = attackAbility.GetTargetsFrom(tp.x, tp.y);

                        List<PathNode> attackablePathNodes = attackPossibilities
                            .Where(p => attackableConditions.All(condition => condition(p)))
                            .ToList();
                        foreach (PathNode attackable in attackablePathNodes)
                        {
                            List<PathNode> movement = movementAbility.GetPathToTargetFrom(pos.x, pos.y, tp.x, tp.y);
                            List<PathNode> attack = attackAbility.GetPathToTargetFrom(tp.x, tp.y, attackable.x, attackable.y);
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
                foreach (Ability movementAbility in movementAbilities)
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

            //foreach(List<Decision> decisions in possibleCoursesOfAction)
            //{
            //    foreach(Decision decision in decisions)
            //    {
            //        ProgramUtils.PrintDecision(decision);
            //    }
            //}

            Profiler.BeginSample("GetAnalysis: SelectStrongestCoursesOfAction");
            List<(List<Decision>, float)> finalPossibleCoursesOfAction = SelectStrongestCoursesOfAction(possibleCoursesOfAction);
            Profiler.EndSample();
            return new Analysis(this, finalPossibleCoursesOfAction.Select(tuple => tuple.Item1).ToList());
        }
        private List<(List<Decision>, float)> SelectStrongestCoursesOfAction(List<List<Decision>> coursesOfAction)
        {
            Profiler.BeginSample("GetAnalysis: SelectStrongestCoursesOfAction");
            List<(List<Decision>, float)> evaluation = coursesOfAction.Select(course => (course, EvaluateStrength(course))).ToList();
            Profiler.BeginSample("GetAnalysis: evaluation.Sort");
            evaluation.Sort((course1, course2) => -course1.Item2.CompareTo(course2.Item2));
            Profiler.EndSample();
            Profiler.EndSample();

            //ProgramUtils.ResetText();
            //foreach ((List<Decision>, float) eval in evaluation)
            //{
            //    foreach (Decision decision in eval.Item1)
            //    {
            //        if (decision.ability.GetType().IsSubclassOf(typeof(MovementAbility)))
            //        {
            //            PathNode finalPos = decision.path.Last();

            //            ProgramUtils.CreateWorldText(finalPos.x, finalPos.y, eval.Item2.ToString());
            //        }
            //    }
            //}
            return evaluation/*.Select(tuple => tuple.Item1)*/.Take(5).ToList();
        }
        private float EvaluateStrength(List<Decision> courseOfAction)
        {
            float movementStrength = 0;
            float damageStrength = 0;
            foreach (Decision decision in courseOfAction)
            {
                if (decision.ability.GetType().IsSubclassOf(typeof(MovementAbility)))
                {
                    Profiler.BeginSample("GetAnalysis: EvaluateMovement");
                    movementStrength = EvaluateMovement(decision);
                    Profiler.EndSample();
                }
                else if (decision.ability.GetType().IsSubclassOf(typeof(OffensiveAbility)))
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
                return (decision.ability as OffensiveAbility).GetDamage();
            }
            return 0;
        }
        private bool IsPlayerActorCondition(PathNode attackablePathNode)
        {
            return pf.DoesPathNodeHavePlayerActor(attackablePathNode);
        }
    }
}