using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class GlobalPersistentDataManager : MonoBehaviour
    {
        private static class GameDataJsonStorage
        {
            private static JObject gameData = new JObject();

            private static readonly string GAME_DATA = "GameData";
            private static readonly string META_GAME_DATA = "MetaGameData";

            public static void LoadSave(int saveId)
            {
                DirectoryInfo directory = Directory.CreateDirectory(Application.persistentDataPath + "/saves");
                string fileName = $"{directory.FullName}/{saveId}.txt";
                if (!File.Exists(fileName))
                {
                    throw new IOException("Missing save file");
                }
                string saveFile = File.ReadAllText($"{directory.FullName}/{saveId}.txt");
                gameData = JsonConvert.DeserializeObject<JObject>(saveFile);
            }
            public static void Save(int saveId)
            {
                SetMetaGameData("Resetted", false);
                SetGameData("CurrentScene", SceneManager.GetActiveScene().name);
                DirectoryInfo directory = Directory.CreateDirectory(Application.persistentDataPath + "/saves");
                try
                {
                    File.WriteAllText($"{directory.FullName}/{saveId}.txt", gameData.ToString());
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }
            public static void SetGameData<T>(string name, T data)
            {
                SetData(name, GAME_DATA, data);
            }
            public static void SetMetaGameData<T>(string name, T data)
            {
                SetData(name, META_GAME_DATA, data);
            }
            public static T GetGameData<T>(string name)
            {
                return GetData<T>(name, GAME_DATA);
            }
            public static T GetMetaGameData<T>(string name)
            {
                return GetData<T>(name, META_GAME_DATA);
            }
            public static void SetData<T>(string name, string category, T data)
            {
                JToken jsonData = JToken.Parse(JsonConvert.SerializeObject(data));
                JProperty prop = new JProperty(name, jsonData);
                if (gameData[category] == null)
                {
                    gameData.Add(category, new JObject());
                    gameData[category][name] = jsonData;
                }
                else
                {
                    gameData[category][name] = jsonData;
                }
            }
            public static T GetData<T>(string name, string category)
            {
                if (gameData[category] != null && gameData[category][name] != null)
                {
                    return gameData[category][name].ToObject<T>();
                }
                else
                {
                    Debug.LogWarning($"Missing value in storage, category {category} data {name} could not be found. Returning default({typeof(T)})");
                    return default;
                }
            }
            public static void ResetGameData()
            {
                gameData[GAME_DATA] = new JObject();
            }
            public static void ResetCache()
            {
                gameData = new JObject();
            }
            public static void LogCache()
            {
                Debug.Log(gameData);
            }
        }
        private GlobalEventManager gem;
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

            try
            {
                GameDataJsonStorage.LoadSave(0);
            }
            catch (IOException e)
            {
                Debug.LogWarning(e);
            }

            if (!GetMetaGameData<bool>("PlayedBefore"))
            {
                SetMetaGameData("PlayedBefore", true);
                SetMetaGameData("Resetted", true);
            }

            JsonConvert.DefaultSettings = GetDefaultJsonSettings;
        }
        public JsonSerializerSettings GetDefaultJsonSettings()
        {
            return new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };
        }
        public static void HandleDeserializationError(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorArgs)
        {
            var currentError = errorArgs.ErrorContext.Error.Message;
            var path = errorArgs.ErrorContext.Path;
            Debug.Log($"Error: {currentError}");
            Debug.Log($"Path: {path}");
            errorArgs.ErrorContext.Handled = true;
        }
        private void Update()
        {
            if (Input.GetKeyDown("i"))
            {
                GameDataJsonStorage.Save(0);
            }
            if (Input.GetKeyDown("o"))
            {
                GameDataJsonStorage.LoadSave(0);
            }
        }

        public T GetGameData<T>(string name)
        {
            return GameDataJsonStorage.GetGameData<T>(name);
        }
        public void SetGameData<T>(string name, T value)
        {
            GameDataJsonStorage.SetGameData(name, value);
        }
        public T GetMetaGameData<T>(string name)
        {
            return GameDataJsonStorage.GetMetaGameData<T>(name);
        }
        public void SetMetaGameData<T>(string name, T value)
        {
            GameDataJsonStorage.SetMetaGameData(name, value);
        }
        public void LoadSave(int saveId)
        {
            GameDataJsonStorage.LoadSave(saveId);
        }
        public void Save(int saveId)
        {
            GameDataJsonStorage.Save(saveId);
        }
        public void Save()
        {
            GameDataJsonStorage.Save(0);
        }
        public void SetupNewGame()
        {
            GameDataJsonStorage.ResetGameData();
        }
        public void _ResetCache()
        {
            GameDataJsonStorage.ResetCache();
        }
        public void _LogCache()
        {
            GameDataJsonStorage.LogCache();
        }
        public bool CheckPrecondition(WorldMap.Precondition prec)
        {
            if (prec.goldCost > GetGameData<int>("Gold"))
            {
                return false;
            }
            if (prec.foodCost > GetGameData<int>("Food"))
            {
                return false;
            }
            return true;
        }
    }
}
