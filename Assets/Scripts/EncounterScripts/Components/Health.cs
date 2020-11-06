using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;
using Sirenix.OdinInspector;

namespace Encounter
{
    public class Health : MonoBehaviour
    {
        private GlobalEventManager gem;
        private Position pos;

        [OnValueChangedAttribute("UpdateHealth")]
        public int maxHealth;
        [HideInInspector]
        public int health;
        private void UpdateHealth()
        {
            health = maxHealth;
        }
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (pos = gameObject.GetComponent<Position>())
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            this.health = maxHealth;
            gem.StartListening("Attack", TakeDamage);
        }

        public void OnDestroy()
        {
            gem.StopListening("Attack", TakeDamage);
        }

        private void TakeDamage(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (pos.x != tx || pos.y != ty)
            {
                return;
            }
            if (parameters.Count == 0)
            {
                throw new Exception("Expected parameter for damage, but found zero parameters");
            }

            int damage = (int)parameters[0];
            health -= damage;
            if (health <= 0)
            {
                gem.TriggerEvent("Death", gameObject, x: pos.x, y: pos.y, tx: tx, ty: ty);
            }
        }

        private void HealDamage(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (invoker != gameObject)
            {
                return;
            }
            if (parameters.Count == 0)
            {
                throw new System.Exception("Expected parameter for heal, but found zero parameters");
            }

            int heal = (int)parameters[0];
            health += heal;

            if (health >= maxHealth)
            {
                health = maxHealth;
            }
        }
    }
}