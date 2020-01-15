using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    private GlobalEventManager gem;
    private Position pos;

    public int maxHealth;
    public int health { get; set; }

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(pos = gameObject.GetComponent<Position>());
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        this.health = maxHealth;
    }

    void Start()
    {
        gem.StartListening("Attack", TakeDamage);
    }

    public void OnDestroy()
    {
        gem.StopListening("Attack", TakeDamage);
    }

    public void TakeDamage(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (pos.x != tx || pos.z != tz)
        {
            return;
        }
        if (parameters.Count == 0)
        {
            throw new System.Exception("Expected parameter for damage, but found zero parameters");
        }

        int damage = (int) parameters[0];
        health -= damage;
        if (health <= 0)
        {
            gem.TriggerEvent("Death", gameObject, x: pos.x, z: pos.z, tx: tx, tz: tz);
        }
    }

    public void HealDamage(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        if (invoker != gameObject)
        {
            return;
        }
        if (parameters.Count == 0)
        {
            throw new System.Exception("Expected parameter for heal, but found zero parameters");
        }

        int heal = (int) parameters[0];
        health += heal;

        if (health >= maxHealth)
        {
            health = maxHealth;
        }
    }
}
