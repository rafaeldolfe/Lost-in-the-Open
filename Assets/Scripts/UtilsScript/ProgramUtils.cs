using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;
using TMPro;

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
        public static void CreateWorldText(int x, int y, string text)
        {
            if (GameObject.Find("Debug") == null)
            {
                new GameObject("Debug");
            }
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            RectTransform transform = gameObject.GetComponent<RectTransform>();
            transform.SetParent(GameObject.Find("Debug").transform);
            transform.Rotate(new Vector3(90, 0, 0));
            transform.localPosition = new Vector3(x, y, 1);// localPosition;
            transform.sizeDelta = new Vector2(1, 1);
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.text = string.Format("{0}", text);
            textMesh.fontSize = 2;

            textMesh.color = Color.black;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = 5000;
        }
        public static void ResetText()
        {
            Component.Destroy(GameObject.Find("Debug"));
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
