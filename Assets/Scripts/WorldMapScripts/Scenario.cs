using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using Newtonsoft.Json;
using Utils;
using Sirenix.OdinInspector;

namespace WorldMap
{
    [Serializable]
    public class Scenario
    {
        public string iconPrefabName;
        public List<TextPrompt> prompts;

        public Scenario(string iconPrefabName, List<TextPrompt> prompts)
        {
            this.iconPrefabName = iconPrefabName;
            this.prompts = prompts;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("");

            res.Append($"iconPrefabName: {iconPrefabName}\n");
            res.Append($"prompts: ");

            for (int i = 0; i < prompts.Count; i++)
            {
                res.Append($"{prompts[i].id}");
                if (i != prompts.Count - 1)
                {
                    res.Append(", ");
                }
            }
            return res.ToString();
        }
    }
    [Serializable]
    public class TextPrompt
    {
        [Tooltip("string id of prompt, used by the \"pointsTo\" property to specify which prompt to jump to.")]
        public string id;
        [Tooltip("Text to display when this prompt is shown.")]
        [MultiLineProperty(8)]
        public string text;
        [Tooltip("The options to display for this prompt.")]
        public List<Option> options;
        [Tooltip("Does this prompt have a conclusion or not? (e.g. 3 food, 4 gold, etc.)")]
        public bool hasConclusion;
        public Conclusion conclusion;
    }
    [Serializable]
    public class Option
    {
        [Tooltip("Description of option.")]
        public string text;
        [Tooltip("Condition for picking this option (e.g. giving 5 food requires 5 food)")]
        public Precondition precondition;
        public Encounter encounter;
        [Tooltip("The prompt this option will jump to. (Post-encounter if encounter exists)")]
        public string pointsTo;

    }
    [Serializable]
    public class Precondition
    {
        public int goldCost;
        public int foodCost;
        [Tooltip("Determines whether to show this option regardless of whether the player satisfies the requirements or not.")]
        public bool showTextRegardless;
    }
    [Serializable]
    public class Encounter
    {
        public string sceneName;
        [JsonIgnore]
        public List<GameObject> enemies;
        [HideInInspector]
        public List<PlainGameObject> plainEnemies;
    }
    [Serializable]
    public class Conclusion
    {
        public int gold;
        public int food;
        [JsonIgnore]
        public List<GameObject> units;
        [HideInInspector]
        public List<PlainGameObject> plainUnits;
    }
}
