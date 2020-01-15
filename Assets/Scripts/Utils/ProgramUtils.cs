using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using System;

public class ProgramUtils
{
    public static Vector2Int GetMouseGridPosition()
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0.5f, 0, 0.5f);

        return new Vector2Int((int) Math.Floor(worldPosition.x), (int) Math.Floor(worldPosition.z));
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

    public static void PrintList<T>(List<T> list)
    {
        foreach(T item in list)
        {
            Debug.Log(item);
        }
    }

    public static void PrintDecision(Decision decision)
    {
        if (decision == null)
        {
            throw new Exception("Missing decision object: Found null");
        }
        if (decision.ability == null)
        {
            throw new Exception("Missing ability object: Found null");
        }
        if (decision.path == null)
        {
            throw new Exception("Missing list of path nodes: Found null");
        }
        if (decision.path.Count == 0)
        {
            throw new Exception("Missing path nodes: Found empty list");
        }
        Debug.Log("Ability: " + decision.ability.GetType());
        Debug.Log(String.Format("Path: ({0},{1}) to ({2},{3})", 
            decision.path.First().x, decision.path.First().z,
            decision.path.Last().x, decision.path.Last().z));
    }
    public static void PrintPathNode(PathNode pathNode)
    {
        if (pathNode == null)
        {
            throw new Exception("Missing decision object: Found null");
        }
        Debug.Log(String.Format("PathNode: ({0},{1})", pathNode.x, pathNode.z));
    }

    public static List<Type> GetMonoBehavioursOnType(Type script)
    {
        return script.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Select(fieldInfo => fieldInfo.FieldType)
            .Where(type => type.IsSubclassOf(typeof(MonoBehaviour)))
            .ToList();
    }
}
