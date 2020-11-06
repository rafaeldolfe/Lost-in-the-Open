using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;
using Utils;
using Encounter;

namespace WorldMap
{
    public class ActorListManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;

        public GameObject actorUIPrefab;
        
        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            gem.StartListening("RegenerateScene", RegenerateUI);
            gem.StartListening("OptionClicked", RegenerateUIParams);
        }

        private void OnDestroy()
        {
            gem.StopListening("RegenerateScene", RegenerateUI);
            gem.StopListening("OptionClicked", RegenerateUIParams);
        }

        private void RegenerateUIParams(GameObject invoker, List<object> parameters)
        {
            RegenerateUI();
        }
        private void RegenerateUI()
        {
            ResetUI();

            List<PlainGameObject> playerUnits = gdm.GetGameData<List<PlainGameObject>>("PlayerUnits");
            PlainGameObject playerKing = gdm.GetGameData<PlainGameObject>("PlayerKing");

            if (playerUnits == null)
            {
                return;
            }
            if (playerKing == null)
            {
                return;
            }

            GenerateActorUI(playerKing);

            foreach (PlainGameObject unit in playerUnits)
            {
                GenerateActorUI(unit);
            }
        }

        private void ResetUI()
        {
            foreach (Transform transform in transform)
            {
                Destroy(transform.gameObject);
            }
        }

        private void GenerateActorUI(PlainGameObject actor)
        {
            GameObject actorUI = Instantiate(actorUIPrefab, transform);

            ActorUIScript actorUIScript = actorUI.GetComponent<ActorUIScript>();

            if (actorUIScript == null)
            {
                throw ProgramUtils.MissingComponentException(typeof(ActorUIScript));
            }

            PlainSprite plainSprite = actor.GetProperty<PlainSprite>(typeof(Actor), "portraitData");
            string name = actor.GetProperty<string>(typeof(Actor), "name");
            int health = actor.GetProperty<int>(typeof(Health), "health");
            int maxHealth = actor.GetProperty<int>(typeof(Health), "maxHealth");
            Sprite sprite = Resources.Load($"{plainSprite.folder}/{plainSprite.spriteName}") as Sprite;

            actorUIScript.SetSprite(sprite);
            actorUIScript.SetName(name);
            actorUIScript.SetHealthPercentage((float)health / (float)maxHealth);
        }
    }
}
