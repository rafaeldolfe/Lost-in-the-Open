using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Utils;

public class InGameMenuManager : MonoBehaviour
{
    private GlobalEventManager gem;

    public GameObject areYouSure;
    public GameObject areYouSureBackground;

    public GameObject menuBackground;
    public Image menuImageBackground;
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
    }
    private bool paused;
    private void BringUpMenu()
    {
        PauseService.AddPauseLevel(PauseService.MENU_PAUSE);
        menuBackground.SetActive(true);
        menuImageBackground.enabled = true;
        foreach(Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        paused = true;
    }
    private void ReturnToGame()
    {
        PauseService.RemovePauseLevel(PauseService.MENU_PAUSE);
        menuBackground.SetActive(false);
        menuImageBackground.enabled = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        paused = false;
    }
    private void TriggerAreYouSure()
    {
        areYouSureBackground.SetActive(true);
        areYouSure.SetActive(true);
    }
    public void ConfirmRestart()
    {
        gem.TriggerEvent("MenuNewGame");
    }
    public void CancelRestart()
    {
        areYouSureBackground.SetActive(false);
        areYouSure.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !paused)
        {
            BringUpMenu();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && paused)
        {
            ReturnToGame();
        }
    }
    public void PressContinue()
    {
        ReturnToGame();
    }
    public void PressRestart()
    {
        TriggerAreYouSure();
    }
    public void PressSettings()
    {
        Debug.Log("Settings...");
    }
    public void PressQuitToMenu()
    {
        gem.TriggerEvent("PrepareQuitToMainMenu");
        gem.TriggerEvent("MenuQuitToMainMenu");
    }
    public void PressQuitToDesktop()
    {
        gem.TriggerEvent("PrepareQuitToDesktop");
        gem.TriggerEvent("MenuQuitToDesktop");
    }
}
