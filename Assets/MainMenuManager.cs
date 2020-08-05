using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Utils;

public class MainMenuManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private GlobalPersistentDataManager gdm;

    public GameObject areYouSure;
    public GameObject areYouSureBackground;

    public GameObject menuBackground;
    public Image menuImageBackground;

    public GameObject continueButton;
    public GameObject newGameButton;
    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (gdm = FindObjectOfType(typeof(GlobalPersistentDataManager)) as GlobalPersistentDataManager),
        };
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }
    private void Start()
    {
        if (gdm.GetMetaGameData<bool>("Resetted"))
        {
            Destroy(continueButton);
            GameObject clone = Instantiate(newGameButton);
            clone.transform.SetParent(transform);
            clone.transform.SetAsFirstSibling();
            clone.transform.localScale = new Vector3(1, 1, 1);
            Destroy(newGameButton);
        }
    }
    private bool paused;
    private void BringUpMenu()
    {
        Debug.Log("Bringing up...");
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
        gem.TriggerEvent("MenuContinue");
    }
    private void TriggerAreYouSure()
    {
        areYouSureBackground.SetActive(true);
        areYouSure.SetActive(true);
    }
    public void ConfirmNewGame()
    {
        gem.TriggerEvent("MenuNewGame");
    }
    public void CancelNewGame()
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
    public void PressNewGame()
    {
        TriggerAreYouSure();
    }
    public void PressSettings()
    {
        Debug.Log("Settings...");
    }
    public void PressQuitToDesktop()
    {
        Debug.Log("QuitToDesktop...");
        gem.TriggerEvent("MenuQuitToDesktop");
    }
}
