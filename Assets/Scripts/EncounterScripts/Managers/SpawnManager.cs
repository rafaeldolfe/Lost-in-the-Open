using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace Encounter
{
    public class SpawnManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private class SpawnLocation
        {
            public Vector2Int position;
            public string type;
            public SpawnLocation(Vector2Int position, string type)
            {
                this.position = position;
                this.type = type;
            }
            public override string ToString()
            {
                return $"type: {type}, position: {position}";
            }
        }
        private List<SpawnLocation> playerSpawnLocations = new List<SpawnLocation>();
        private List<SpawnLocation> enemySpawnLocations = new List<SpawnLocation>();
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("RespawnPlayerUnits", RespawnPlayerUnits);
            gem.StartListening("RespawnEnemyUnits", RespawnEnemyUnits);

            gem.StartListening("SpawnPlayerUnits", SpawnPlayerUnits);
            gem.StartListening("SpawnEnemyUnits", SpawnEnemyUnits);
        }
        void OnDestroy()
        {
            gem.StopListening("RespawnPlayerUnits", RespawnPlayerUnits);
            gem.StopListening("RespawnEnemyUnits", RespawnEnemyUnits);

            gem.StopListening("SpawnPlayerUnits", SpawnPlayerUnits);
            gem.StopListening("SpawnEnemyUnits", SpawnEnemyUnits);
        }
        private void RespawnPlayerUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new Exception(string.Format("Expected list with 2 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<PlainGameObject>, found ", parameters[0].GetType()));
            }
            if (parameters[1].GetType() != typeof(PlainGameObject))
            {
                throw new Exception(string.Format("Expected 2nd item to be PlainGameObject, found ", parameters[1].GetType()));
            }
            List<GameObject> units = ((List<PlainGameObject>)parameters[0]).InstantiateSelves(Instantiate);
            GameObject king = ((PlainGameObject)parameters[1]).InstantiateSelf(Instantiate);

            gem.TriggerEvent("RegisterPlayerUnits", gameObject, new List<object> { units, king });

            foreach (GameObject unit in units)
            {
                Position pos = unit.GetComponent<Position>();
                gem.TriggerEvent("RegisterUnit", unit, new List<object> { new Vector2Int(pos.x, pos.y) });
            }

            Position kingPos = king.GetComponent<Position>();
            gem.TriggerEvent("RegisterUnit", king, new List<object> { new Vector2Int(kingPos.x, kingPos.y) });
        }
        private void RespawnEnemyUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected list with 1 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<PlainGameObject>, found ", parameters[0].GetType()));
            }
            List<GameObject> enemyUnits = ((List<PlainGameObject>)parameters[0]).InstantiateSelves(Instantiate);

            gem.TriggerEvent("RegisterEnemyUnits", gameObject, new List<object> { enemyUnits });

            foreach (GameObject unit in enemyUnits)
            {
                Position pos = unit.GetComponent<Position>();
                gem.TriggerEvent("RegisterUnit", unit, new List<object> { new Vector2Int(pos.x, pos.y) });
            }
        }
        private void SpawnPlayerUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 3)
            {
                throw new Exception(string.Format("Expected list with 3 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<PlainGameObject>, found ", parameters[0].GetType()));
            }
            if (parameters[1].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 2nd item to be List<PlainGameObject>, found ", parameters[1].GetType()));
            }
            if (parameters[2].GetType() != typeof(PlainGameObject))
            {
                throw new Exception(string.Format("Expected 3rd item to be PlainGameObject, found ", parameters[2].GetType()));
            }
            List<GameObject> melees = ((List<PlainGameObject>)parameters[0]).InstantiateSelves(Instantiate);
            List<GameObject> rangeds = ((List<PlainGameObject>)parameters[1]).InstantiateSelves(Instantiate);
            GameObject king = ((PlainGameObject)parameters[2]).InstantiateSelf(Instantiate);

            gem.TriggerEvent("RegisterPlayerUnits", gameObject, new List<object> { melees.Concat(rangeds).ToList(), king });

            playerSpawnLocations = playerSpawnLocations.Shuffle();
            Queue<SpawnLocation> meleeSpawnLocations = new Queue<SpawnLocation>(playerSpawnLocations.Where(loc => loc.type == "Melee").ToList());
            foreach (GameObject melee in melees)
            {
                if (meleeSpawnLocations.Count > 0)
                {
                    SpawnLocation spawnLocation = meleeSpawnLocations.Dequeue();
                    gem.TriggerEvent("RegisterUnit", melee, new List<object> { spawnLocation.position });
                }
                else
                {
                    throw new Exception("Invalid parameter: More player units to spawn than player spawn locations on map");
                }
            }
            Queue<SpawnLocation> rangedSpawnLocations = new Queue<SpawnLocation>(playerSpawnLocations.Where(loc => loc.type == "Ranged").ToList());
            foreach (GameObject ranged in rangeds)
            {
                if (rangedSpawnLocations.Count > 0)
                {
                    SpawnLocation spawnLocation = rangedSpawnLocations.Dequeue();
                    gem.TriggerEvent("RegisterUnit", ranged, new List<object> { spawnLocation.position });
                }
                else
                {
                    throw new Exception("Invalid parameter: More player units to spawn than player spawn locations on map");
                }
            }
            Queue<SpawnLocation> kingSpawnLocations = new Queue<SpawnLocation>(playerSpawnLocations.Where(loc => loc.type == "King").ToList());
            if (kingSpawnLocations.Count > 0)
            {
                SpawnLocation spawnLocation = kingSpawnLocations.Dequeue();
                gem.TriggerEvent("RegisterUnit", king, new List<object> { spawnLocation.position });
            }
            else
            {
                throw new Exception("Invalid parameter: More player units to spawn than player spawn locations on map");
            }
        }
        private void SpawnEnemyUnits(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 2)
            {
                throw new Exception(string.Format("Expected list with 2 items, found {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 1st item to be List<PlainGameObject>, found ", parameters[0].GetType()));
            }
            if (parameters[1].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected 2nd item to be List<PlainGameObject>, found ", parameters[1].GetType()));
            }

            List<GameObject> melees = ((List<PlainGameObject>)parameters[0]).InstantiateSelves(Instantiate);
            List<GameObject> rangeds = ((List<PlainGameObject>)parameters[1]).InstantiateSelves(Instantiate);

            gem.TriggerEvent("RegisterEnemyUnits", gameObject, new List<object> { melees.Concat(rangeds).ToList() });

            enemySpawnLocations = enemySpawnLocations.Shuffle();
            Queue<SpawnLocation> meleeSpawnLocations = new Queue<SpawnLocation>(enemySpawnLocations.Where(loc => loc.type == "Melee").ToList());
            
            foreach (GameObject melee in melees)
            {
                if (meleeSpawnLocations.Count > 0)
                {
                    SpawnLocation spawnLocation = meleeSpawnLocations.Dequeue();
                    gem.TriggerEvent("RegisterUnit", melee, new List<object> { spawnLocation.position });
                }
                else
                {
                    throw new Exception("Invalid parameter: More player units to spawn than player spawn locations on map");
                }
            }
            Queue<SpawnLocation> rangedSpawnLocations = new Queue<SpawnLocation>(enemySpawnLocations.Where(loc => loc.type == "Ranged").ToList());
            foreach (GameObject ranged in rangeds)
            {
                if (rangedSpawnLocations.Count > 0)
                {
                    SpawnLocation spawnLocation = rangedSpawnLocations.Dequeue();
                    gem.TriggerEvent("RegisterUnit", ranged, new List<object> { spawnLocation.position });
                }
                else
                {
                    throw new Exception("Invalid parameter: More player units to spawn than player spawn locations on map");
                }
            }
        }
        public void AddPlayerSpawnLocation(Vector2Int position, string type)
        {
            if (!Constants.UNIT_TYPES.Contains(type))
            {
                throw new Exception("Invalid argument: Tried to add spawn location of the non-existing unit type " + type);
            }
            playerSpawnLocations.Add(new SpawnLocation(position, type));
        }
        public void AddEnemySpawnLocation(Vector2Int position, string type)
        {
            if (!Constants.UNIT_TYPES.Contains(type))
            {
                throw new Exception("Invalid argument: Tried to add spawn location of the non-existing unit type " + type);
            }
            enemySpawnLocations.Add(new SpawnLocation(position, type));
        }
    }
}