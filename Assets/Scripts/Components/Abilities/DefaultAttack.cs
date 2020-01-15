using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[RequireComponent(typeof(AbilitiesHandler))]
[RequireComponent(typeof(Position))]
public class DefaultAttack : Ability
{
    private GlobalEventManager gem;
    private AbilitiesHandler ah;
    private Position pos;

    public int defaultAttackDamage;
    public int numberOfAttacks;

    private int remainingAttacks;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(ah = gameObject.GetComponent<AbilitiesHandler>());
        deps.Add(pos = gameObject.GetComponent<Position>());
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        remainingAttacks = numberOfAttacks;
        category = "Attack";
        highlightColor = new Color(0.75f, 0.29f, 0.22f, 0.78f); // Red
        pfconfig = new PathfindingConfig(ignoresTerrain: false, ignoresActors: true);
        gem.StartListening("Attack", RegisterAttackHandler);
    }

    public void OnDestroy()
    {
        gem.StopListening("Attack", RegisterAttackHandler);
    }
    public void RegisterAttackHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
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

        gem.TriggerEvent("Attack", gameObject, new List<object> { defaultAttackDamage }, pos.x, pos.z, last.x, last.z);
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
}
