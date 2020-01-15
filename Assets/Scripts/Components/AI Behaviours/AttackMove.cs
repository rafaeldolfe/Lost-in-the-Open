using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(Position))]
[RequireComponent(typeof(BehaviourHandler))]
[RequireComponent(typeof(AbilitiesHandler))]

public class AttackMove : Behaviour
{
    private FactionManager fm;
    private BehaviourHandler bh;
    private Pathfinding pf;
    private Position pos;

    private List<Func<PathNode, bool>> attackableConditions;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (fm = FindObjectOfType(typeof(FactionManager)) as FactionManager),
            (pf = FindObjectOfType(typeof(Pathfinding)) as Pathfinding),
            (bh = gameObject.GetComponent<BehaviourHandler>()),
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
        attackableConditions.Add(IsPlayerActorCondition);
    }
    public override Analysis GetAnalysis(List<Ability> abilities)
    {
        List<Ability> movementAbilities = abilities
            .Where(ability => ability.category == "Movement")
            .ToList();
        List<Ability> attackAbilities = abilities
            .Where(ability => ability.category == "Attack")
            .ToList();

        List<List<Decision>> possibleCoursesOfAction = new List<List<Decision>>();
        foreach (Ability movementAbility in movementAbilities)
        {
            List<PathNode> movementPossibilities = pf.FindPathNodesWithinRange(pos.x, pos.z, movementAbility.GetRange(), movementAbility.pfconfig);
            foreach (PathNode tp in movementPossibilities)
            {
                foreach(Ability attackAbility in attackAbilities)
                {
                    List<PathNode> attackPossibilities = pf.FindPathNodesWithinRange(tp.x, tp.z, attackAbility.GetRange(), attackAbility.pfconfig);

                    List<PathNode> attackablePathNodes = attackPossibilities
                        .Where(p => attackableConditions.All(condition => condition(p)))
                        .ToList();
                    foreach(PathNode attackable in attackablePathNodes)
                    {
                        List<PathNode> movement = pf.FindPath(pos.x, pos.z, tp.x, tp.z, movementAbility.pfconfig);
                        List<PathNode> attack = pf.FindPath(tp.x, tp.z, attackable.x, attackable.z, attackAbility.pfconfig);
                        List<Decision> courseOfAction = new List<Decision> {
                            new Decision(movementAbility, movement),
                            new Decision(attackAbility, attack)
                        };

                        possibleCoursesOfAction.Add(courseOfAction);
                    }
                }
            }
        }
        if (possibleCoursesOfAction.Count == 0)
        {
            foreach (Ability movementAbility in movementAbilities)
            {
                PathNode pa = pf.GetClosestPlayerActorPosition(fm.GetFaction("Player"), pos.x, pos.z);

                PathNode curr = pf.GetNode(pos.x,pos.z);
                int minDistance = pf.FindPath(pos.x, pos.z, pa.x, pa.z).Count - 1;
                List<PathNode> movementPossibilities = pf.FindPathNodesWithinRange(pos.x, pos.z, movementAbility.GetRange(), movementAbility.pfconfig);
                foreach (PathNode tp in movementPossibilities)
                {
                    int distance = pf.FindPath(tp.x, tp.z, pa.x, pa.z).Count - 1;
                    if (distance < minDistance)
                    {
                        Debug.Log(String.Format("({0},{1}) is closer than ({2},{3})", tp.x, tp.z, curr.x, curr.z));
                        curr = tp;
                        minDistance = distance;
                    }
                }
                List<PathNode> finalPath = pf.FindPath(pos.x, pos.z, curr.x, curr.z, movementAbility.pfconfig);
                Decision decision = new Decision(movementAbility, finalPath);
                possibleCoursesOfAction.Add(new List<Decision> { decision });
            }
        }

        return new Analysis(this, possibleCoursesOfAction);
    }
    private bool IsPlayerActorCondition(PathNode attackablePathNode)
    {
        return pf.DoesPathNodeHavePlayerActor(attackablePathNode);
    }
}
