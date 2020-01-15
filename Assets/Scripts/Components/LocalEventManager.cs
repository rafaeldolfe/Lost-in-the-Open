using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LocalEventManager : MonoBehaviour
{
    private Dictionary<string, Action<GameObject, int, int, int, int, List<object>>> eventDictionary;

    public void StartListening(string eventName, Action<GameObject, int, int, int, int, List<object>> listener)
    {
        Action<GameObject, int, int, int, int, List<object>> thisEvent = null;
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] += listener;
        }
        else
        {
            eventDictionary.Add(eventName, listener);
        }
    }

    public void StopListening(string eventName, Action<GameObject, int, int, int, int, List<object>> listener)
    {
        Action<GameObject, int, int, int, int, List<object>> thisEvent = null;
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
        }
    }

    public void TriggerEvent(string eventName, GameObject invoker, int x, int z, int tx, int tz, List<object> parameters)
    {
        Action<GameObject, int, int, int, int, List<object>> thisEvent = null;
        if (eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(invoker, x, z, tx, tz, parameters);
        }
    }
}
