using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Utils;

namespace WorldMap
{
    public class WorldMapPlayerManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;
        private WorldMapManager wmm;

        public GameObject spawnPoint;
        public GameObject playerPrefab;
        public float moveSpeed;

        private Location playerLocation;
        private GameObject player;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
                (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
                (wmm = FindObjectOfType(typeof(WorldMapManager)) as WorldMapManager),
            };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("RegeneratedMap", RespawnPlayer);
            gem.StartListening("GeneratedMap", SpawnPlayer);
            gem.StartListening("ClickNode", AttemptMovePlayer);
        }
        private void OnDestroy()
        {
            gem.StopListening("RegeneratedMap", RespawnPlayer);
            gem.StopListening("GeneratedMap", SpawnPlayer);
            gem.StopListening("ClickNode", AttemptMovePlayer);
        }
        public void _DestroyPlayer()
        {
            Destroy(player);
        }
        private void RespawnPlayer(GameObject invoker, List<object> parameters)
        {
            playerLocation = gdm.GetGameData<Location>("PlayerLocation");
            player = Instantiate(playerPrefab, new Vector3(playerLocation.position.x, playerLocation.position.y), Quaternion.identity);
            SetPlayerLocation(playerLocation);
        }
        private void SpawnPlayer(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception($"Expected list of location, found list had {parameters.Count} items");
            }
            if (parameters[0].GetType() != typeof(List<Location>))
            {
                throw new Exception($"Expected first object to be List<Location, found {parameters[0].GetType()}");
            }
            List<Location> locations = (List<Location>)parameters[0];
            playerLocation = FindNearestLocation(spawnPoint, locations);
            player = Instantiate(playerPrefab, new Vector3(playerLocation.position.x, playerLocation.position.y), Quaternion.identity);
            SetPlayerLocation(playerLocation);
        }
        private bool moving;
        private void AttemptMovePlayer(GameObject targetNode, List<object> parameters)
        {
            if (moving) return;
            if (wmm.CheckIfLocationAndNodeAreNeighbours(playerLocation, targetNode))
            {
                StartCoroutine(MoveOverSpeed(targetNode, moveSpeed));
            }
        }
        public IEnumerator MoveOverSpeed(GameObject node, float speed)
        {
            moving = true;
            // speed should be 1 unit per second
            while (player.transform.position != node.transform.position)
            {
                if (!PauseService.IsLevelPaused(PauseService.MENU_PAUSE))
                {
                    player.transform.position = Vector3.MoveTowards(player.transform.position, node.transform.position, speed * Time.deltaTime);
                }
                yield return new WaitForEndOfFrame();
            }
            playerLocation = wmm.GetLocation(node);
            gdm.SetGameData("PlayerLocation", playerLocation);

            gem.TriggerEvent("Arrived", node);
            moving = false;
        }
        private void SetPlayerLocation(Location playerLocation)
        {
            player.transform.position = new Vector3(playerLocation.position.x, playerLocation.position.y);
            gdm.SetGameData("PlayerLocation", playerLocation);
        }
        private Location FindNearestLocation(Vector3 position, List<Location> locations)
        {
            return locations
                .OrderBy(loc => (new Vector3(loc.position.x, loc.position.y) - position).sqrMagnitude)
                .First();
        }
        private Location FindNearestLocation(GameObject gameObject, List<Location> locations)
        {
            return FindNearestLocation(gameObject.transform.position, locations);
        }
    }
}
