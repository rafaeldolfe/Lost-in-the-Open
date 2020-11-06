using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using System;
using WorldMap;

namespace Utils
{
    public class GlobalSceneManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;
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
            gem.StartListening("StartEncounter", LoadEncounterScene);
            gem.StartListening("EncounterWin", LoadPostEncounter);
            gem.StartListening("MenuNewGame", LoadNewGame);
            gem.StartListening("MenuContinue", LoadContinue);
            gem.StartListening("MenuQuitToMainMenu", LoadMainMenu);
        }

        private void OnDestroy()
        {
            gem.StopListening("StartEncounter", LoadEncounterScene);
            gem.StopListening("EncounterWin", LoadPostEncounter);
            gem.StopListening("MenuNewGame", LoadNewGame);
            gem.StopListening("MenuContinue", LoadContinue);
            gem.StopListening("MenuQuitToMainMenu", LoadMainMenu);
        }
        private void LoadMainMenu(GameObject invoker, List<object> parameters)
        {
            gdm.Save();
            StartCoroutine(LoadMainMenu());
        }
        private IEnumerator LoadMainMenu()
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Constants.MAIN_MENU);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        private void LoadContinue(GameObject invoker, List<object> parameters)
        {
            PauseService.Unpause();
            StartCoroutine(LoadContinue());
        }
        private IEnumerator LoadContinue()
        {
            string currentScene = gdm.GetGameData<string>("CurrentScene");
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(currentScene);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            gem.TriggerEvent("RegenerateScene");
        }
        private void LoadNewGame(GameObject invoker, List<object> parameters)
        {
            PauseService.Unpause();
            StartCoroutine(LoadNewGame());
        }
        private IEnumerator LoadNewGame()
        {
            gdm.SetupNewGame();

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Constants.WORLD_MAP_SCENE_NAME);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            gem.TriggerEvent("StartNewGame");
        }
        private void LoadPostEncounter(GameObject invoker, List<object> parameters)
        {
            TextPrompt prompt = gdm.GetGameData<TextPrompt>("PostFightPrompt");

            StartCoroutine(LoadPostEncounter(Constants.WORLD_MAP_SCENE_NAME, prompt));
        }
        private IEnumerator LoadPostEncounter(string sceneName, TextPrompt prompt)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            gem.TriggerEvent("RegenerateScene", gameObject);

            gem.TriggerEvent("ShowPostEncounterPrompt", gameObject, new List<object> { prompt });
        }

        private void LoadEncounterScene(GameObject invoker, List<object> parameters)
        {
            if (parameters.Count != 3)
            {
                throw new Exception($"Expected scene name, list of enemies and a TextPrompt, found list had {parameters.Count} items");
            }
            if (parameters[0].GetType() != typeof(string))
            {
                throw new Exception(string.Format("Expected first object to be string, found {0}", parameters[0].GetType()));
            }
            if (parameters[1].GetType() != typeof(List<PlainGameObject>))
            {
                throw new Exception(string.Format("Expected second object to be List<PlainGameObject>, found {0}", parameters[0].GetType()));
            }
            if (parameters[2].GetType() != typeof(TextPrompt))
            {
                throw new Exception(string.Format("Expected third object to be TextPrompt, found {0}", parameters[0].GetType()));
            }
            string sceneName = (string)parameters[0];
            List<PlainGameObject> plainEnemies = (List<PlainGameObject>)parameters[1];
            TextPrompt prompt = (TextPrompt)parameters[2];

            StartCoroutine(LoadEncounterScene(sceneName, plainEnemies, prompt));
        }
        private IEnumerator LoadEncounterScene(string sceneName, List<PlainGameObject> plainEnemies, TextPrompt prompt)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            gdm.SetGameData("PostFightPrompt", prompt);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            gem.TriggerEvent("GenerateEncounter", gameObject, new List<object> { plainEnemies });
        }
    }
}