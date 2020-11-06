using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using System;
using TMPro;
using Utils;

namespace WorldMap
{
    public class TextPromptManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private GlobalPersistentDataManager gdm;

        public ScenarioScriptableObject newGameScenario;
        public GameObject king;

        public GameObject descriptionPrefab;
        public GameObject optionPrefab;
        public GameObject conclusionPrefab;

        private Dictionary<int, UnityAction> optionAction = new Dictionary<int, UnityAction>();

        public Image background;
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
            background.enabled = false;
            gem.StartListening("StartScenario", StartScenario);
            gem.StartListening("StartNewGame", ShowNewGamePrompt);
            gem.StartListening("RegenerateScene", RegeneratePrompt);
            gem.StartListening("ShowPostEncounterPrompt", ShowPostEncounterPrompt);
            gem.StartListening("MenuQuitToMainMenu", Clear);
            gem.StartListening("OptionClicked", OptionClicked);
        }

        private void OnDestroy()
        {
            gem.StopListening("StartScenario", StartScenario);
            gem.StopListening("StartNewGame", ShowNewGamePrompt);
            gem.StopListening("RegenerateScene", RegeneratePrompt);
            gem.StopListening("ShowPostEncounterPrompt", ShowPostEncounterPrompt);
            gem.StopListening("MenuQuitToMainMenu", Clear);
            gem.StopListening("OptionClicked", OptionClicked);
        }

        private void ShowPostEncounterPrompt(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected 1 TextPrompt, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(TextPrompt))
            {
                throw new Exception(string.Format("Expected first object to be TextPrompt, found {0}", parameters[0].GetType()));
            }

            TextPrompt prompt = (TextPrompt)parameters[0];

            LoadPrompt(prompt);
        }
        private void ShowNewGamePrompt()
        {
            gdm.SetGameData("PlayerKing", king.GetPlainClass());

            StartScenario(newGameScenario.GetPlainClass());
        }
        private void StartScenario(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected event asset, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(Scenario))
            {
                throw new Exception(string.Format("Expected first object to be Scenario, found {0}", parameters[0].GetType()));
            }
            Scenario scenario = (Scenario)parameters[0];
            if (scenario.prompts.Count() == 0)
            {
                throw new Exception(string.Format("Expected scenario to have a least 1 prompt, found 0: {0}", scenario));
            }

            StartScenario(scenario);
        }

        private void StartScenario(Scenario scenario)
        {
            TextPrompt tp = scenario.prompts.First();

            gdm.SetGameData("CurrentScenario", scenario);

            LoadPrompt(tp);
        }

        private void LoadPrompt(TextPrompt prompt)
        {
            ClearPrompt();
            PauseService.AddPauseLevel(PauseService.TEXT_PAUSE);
            background.enabled = true;
            GameObject description = Instantiate(descriptionPrefab, transform);
            GameObject option;
            description.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = prompt.text;

            if (prompt.hasConclusion)
            {
                Conclusion conclusion = prompt.conclusion;
                if (conclusion.gold != 0)
                {
                    GameObject conclusionClone = Instantiate(conclusionPrefab, transform);
                    conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Gold: {conclusion.gold}";
                    conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Constants.COLOR_GOLD;
                    if (conclusion.gold < 0)
                    {
                        conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                    }
                    if (gdm.GetGameData<string>("CurrentPromptId") != prompt.id)
                    {
                        gem.TriggerEvent("AddGold", gameObject, new List<object> { conclusion.gold });
                    }
                }
                if (conclusion.food != 0)
                {
                    GameObject conclusionClone = Instantiate(conclusionPrefab, transform);
                    conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"Food: {conclusion.food}";
                    conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Constants.COLOR_FOOD;
                    if (conclusion.food < 0)
                    {
                        conclusionClone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                    }
                    if (gdm.GetGameData<string>("CurrentPromptId") != prompt.id)
                    {
                        gem.TriggerEvent("AddFood", gameObject, new List<object> { conclusion.food });
                    }
                }
                if (conclusion.plainUnits.Count != 0)
                {
                    if (gdm.GetGameData<string>("CurrentPromptId") != prompt.id)
                    {
                        gem.TriggerEvent("AddUnits", gameObject, new List<object> { conclusion.plainUnits });
                    }
                }
            }

            if (prompt.options.Count == 0)
            {
                option = Instantiate(optionPrefab, transform);
                option.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Continue";
                optionAction[0] = () => FinishPrompt();
                if (prompt.id == "WinPrompt")
                {
                    Debug.Log("Yes");
                    optionAction[0] = () => FinishPrompt();
                    optionAction[0] = optionAction[0] + (() => gem.TriggerEvent("WinGame", gameObject));
                }
            }

            int i = 0;
            int debugI = 0;
            List<Option> options = new List<Option>(prompt.options);
            Scenario currentScenario = gdm.GetGameData<Scenario>("CurrentScenario");
            while (i < options.Count || debugI > 1000)
            {
                debugI++;
                Option opt = options[i];
                bool isValidOption = gdm.CheckPrecondition(opt.precondition);
                if (!isValidOption)
                {
                    if (opt.precondition.showTextRegardless)
                    {
                        option = Instantiate(optionPrefab, transform);
                        option.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{i + 1}. {opt.text}";
                        option.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.red;
                    }
                    else
                    {
                        options.RemoveAt(i);
                        continue;
                    }
                }
                else
                {
                    option = Instantiate(optionPrefab, transform);
                    option.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{i + 1}. {opt.text}";
                    if (opt.encounter.sceneName != "")
                    {
                        optionAction[i] = () => LoadEncounter(opt.encounter.sceneName, opt.encounter.plainEnemies, GetPromptById(currentScenario, opt.pointsTo));
                        optionAction[i] = optionAction[i] + (() => FinishPrompt());
                    }
                    else
                    {
                        optionAction[i] = () => LoadPrompt(GetPromptById(currentScenario, opt.pointsTo));
                    }
                }
                i++;
            }
            gdm.SetGameData("CurrentPromptId", prompt.id);
        }
        private void LoadEncounter(string scene, List<PlainGameObject> enemies, TextPrompt prompt)
        {
            gem.TriggerEvent("StartEncounter", gameObject, new List<object> { scene, enemies, prompt });
        }
        private void OptionClicked(GameObject option, List<object> parameters)
        {
            if (parameters.Count != 1)
            {
                throw new Exception(string.Format("Expected index, found list had {0} items", parameters.Count));
            }
            if (parameters[0].GetType() != typeof(int))
            {
                throw new Exception(string.Format("Expected first object to be int, found {0}", parameters[0].GetType()));
            }
            int index = (int)parameters[0];

            if (optionAction.ContainsKey(index))
            {
                optionAction[index]();
            }
        }
        private void RegeneratePrompt()
        {
            string currentPromptId = gdm.GetGameData<string>("CurrentPromptId");
            Scenario currentScenario = gdm.GetGameData<Scenario>("CurrentScenario");
            if (currentPromptId == "" || currentScenario == null)
            {
                return;
            }
            LoadPrompt(GetPromptById(currentScenario, currentPromptId));
        }

        private TextPrompt GetPromptById(Scenario scenario, string id)
        {
            TextPrompt next = scenario.prompts.Find(tp => tp.id == id);
            if (next == null)
            {
                throw new Exception($"Invalid parameter: Option pointer {id} points to nothing");
            }
            return next;
        }
        private void ClearPrompt()
        {
            PauseService.RemovePauseLevel(PauseService.TEXT_PAUSE);
            background.enabled = false;
            optionAction.Clear();
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
        private void Clear(GameObject invoker, List<object> parameters)
        {
            ClearPrompt();
        }
        private void FinishPrompt()
        {
            gdm.SetGameData("CurrentPromptId", "");
            ClearPrompt();
        }
    }
}
