using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgramDebug : MonoBehaviour
{
    public static bool debug = true;

    public static void PrintList<T>(List<T> l)
    {
        foreach(T item in l)
        {
            Debug.Log(item);
        }
    }
}
