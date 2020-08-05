using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Utils;

public class WinStateManager : MonoBehaviour
{
    private GlobalEventManager gem;

    public GameObject winPrompt;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>
            {
                (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            };
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
        gem.StartListening("WinGame", WinGame);
    }
    private void OnDestroy()
    {
        gem.StopListening("WinGame", WinGame);
    }

    private void WinGame()
    {
        winPrompt.SetActive(true);
    }
}
