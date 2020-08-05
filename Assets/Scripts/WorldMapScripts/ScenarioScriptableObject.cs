using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System;

namespace WorldMap
{
    [Serializable]
    [CreateAssetMenu]
    public class ScenarioScriptableObject : ScriptableObject
    {
        public string iconPrefabName;
        public List<TextPrompt> prompts;
        public Scenario GetPlainClass()
        {
            foreach (TextPrompt prompt in prompts)
            {
                prompt.conclusion.plainUnits = prompt.conclusion.units.GetPlainClasses();
                foreach (Option option in prompt.options)
                {
                    option.encounter.plainEnemies = option.encounter.enemies.GetPlainClasses();
                }
            }
            return new Scenario(iconPrefabName, prompts);
        }
    }
}
