using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using TMPro;
using Encounter;

namespace Utils
{
    public class ProgramUtils
    {
        public static Vector2Int GetMouseGridPosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            return new Vector2Int((int)Math.Round(worldPosition.x), (int)Math.Round(worldPosition.y));
        }

        public static Exception DependencyException(List<MonoBehaviour> deps, List<Type> depTypes)
        {
            if (deps.Count != depTypes.Count)
            {
                throw new Exception("List of dependencies and list of respective dependency types must have equal length");
            }
            if (deps.Count == 0)
            {
                throw new Exception("Expected list of dependencies, got empty list (dude I can't make u a dependency exception without dependencies)");
            }
            string text = string.Format("Expected {0} dependencies, missing ", deps.Count);
            for (int i = 0; i < deps.Count; i++)
            {
                if (deps[i] == null)
                {
                    text = text + depTypes[i] + ", ";
                }
            }
            return new Exception(text);
        }
        public static Exception MissingComponentException(Type component)
        {
            string text = string.Format("Missing component {0}", component);
            return new Exception(text);
        }
        public static void PrintList<T>(List<T> list)
        {
            foreach (T item in list)
            {
                Debug.Log(item);
            }
        }
        public static void PrintIEnumerable<T>(IEnumerable<T> list)
        {
            foreach (T item in list)
            {
                Debug.Log(item);
            }
        }
        public static List<Type> GetMonoBehavioursOnType(Type script)
        {
            return script.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(fieldInfo => fieldInfo.FieldType)
                .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)))
                .ToList();
        }
    }
}
