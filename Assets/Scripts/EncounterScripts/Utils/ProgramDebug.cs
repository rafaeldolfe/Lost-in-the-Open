using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace Encounter
{
    public class ProgramDebug : MonoBehaviour
    {
        public static bool debug = true;

        public static void PrintList<T>(List<T> l)
        {
            foreach (T item in l)
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
                decision.path.First().x, decision.path.First().y,
                decision.path.Last().x, decision.path.Last().y));
        }
        public static void PrintPathNode(PathNode pathNode)
        {
            if (pathNode == null)
            {
                throw new Exception("Missing decision object: Found null");
            }
            Debug.Log(String.Format("PathNode: ({0},{1})", pathNode.x, pathNode.y));
        }
        public static void HighlightPathNodes(List<PathNode> pathNodes, Color color = default)
        {
            color = color != default ? color : Color.white;
            foreach (PathNode pathNode in pathNodes)
            {
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.GetComponent<MeshRenderer>().material.color = color;
                cube.transform.position = new Vector3(pathNode.x, pathNode.y, 1);
                cube.transform.localScale = new Vector3(0.25f, 0.25f, 1);
                //cube.transform.SetParent(GameObject.Find("Debug").transform);
            }
        }
    }
}