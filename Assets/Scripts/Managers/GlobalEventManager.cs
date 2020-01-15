using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GlobalEventManager : MonoBehaviour
{

    private Dictionary<string, Action<GameObject, List<object>, int, int, int, int>> eventDictionary;

    void Awake()
    {
        eventDictionary = new Dictionary<string, Action<GameObject, List<object>, int, int, int, int>>();
    }

    public void StartListening(string eventName, Action<GameObject, List<object>, int, int, int, int> listener)
    {
        Action<GameObject, List<object>, int, int, int, int> thisEvent;
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] += listener;
        }
        else
        {
            eventDictionary.Add(eventName, listener);
        }
    }

    public void StopListening(string eventName, Action<GameObject, List<object>, int, int, int, int> listener)
    {
        Action<GameObject, List<object>, int, int, int, int> thisEvent;
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
        }
    }

    public void TriggerEvent(string eventName, GameObject invoker, List<object> parameters = null, int x = -1, int z = -1, int tx = -1, int tz = -1)
    {
        parameters = parameters ?? new List<object>();
        Action<GameObject, List<object>, int, int, int, int> thisEvent;
        if (eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(invoker, parameters, x, z, tx, tz);
        }
    }
}

