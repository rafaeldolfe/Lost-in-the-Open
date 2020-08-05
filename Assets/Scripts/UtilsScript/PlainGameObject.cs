using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System;
using Newtonsoft.Json;
using Sirenix.Serialization;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

namespace Utils
{
    [Serializable]
    public class PlainGameObject
    {
        [Serializable]
        public class ScriptProperties
        {
            public string type;
            public Dictionary<string, object> properties;
        }
        /// <summary>
        /// This exact prefab's name
        /// </summary>
        public string prefabName;
        /// <summary>
        /// The path to this prefab's location in the resource folder
        /// </summary>
        public string folder;
        public List<ScriptProperties> scripts;

        /// <summary>
        /// Instantiate this object's associated GameObject.
        /// Must provide the Instantiate function from a Unity object (e.g. MonoBehaviour)
        /// </summary>
        /// <param name="instantiateFunc"></param>
        /// <returns></returns>
        public GameObject InstantiateSelf(Func<GameObject, GameObject> instantiateFunc)
        {
            GameObject go = Resources.Load($"{folder}/{prefabName}") as GameObject;
            if (go == null)
            {
                throw new Exception($"Could not find in resources: {folder}/{prefabName}");
            }
            GameObject clone = instantiateFunc(go);

            List<MonoBehaviour> monos = clone.GetComponents<MonoBehaviour>().ToList();

            foreach (ScriptProperties props in scripts)
            {
                Type current = Type.GetType(props.type);
                foreach (MonoBehaviour mono in monos)
                {
                    if (current == mono.GetType())
                    {
                        string propsDictToJsonObject = JsonConvert.SerializeObject(props.properties);
                        JsonUtility.FromJsonOverwrite(propsDictToJsonObject, mono);
                    }
                }
            }

            return clone;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder($"prefabName: {prefabName}\n");
            sb.Append("scripts: \n");
            if (scripts.Count() > 0)
            {
                scripts.ForEach(script => sb.Append($"type: {script.type}\n properties.Count(): {script.properties.Count()}\n"));
            }
            return sb.ToString();
        }
    }
}