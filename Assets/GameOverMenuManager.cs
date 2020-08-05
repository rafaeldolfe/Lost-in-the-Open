using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Utils;

public class GameOverMenuManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private GlobalPersistentDataManager gdm;

    public GameObject gameOverBackground;
    public Image gameOverImageBackground;
    public TextMeshProUGUI gameOverText;

    private string GAME_LOST_STRING;
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
        gem.StartListening("GameOver", GameOverLoss);
        gem.StartListening("WinGame", GameOverWin);
    }
    private void OnDestroy()
    {
        gem.StopListening("GameOver", GameOverLoss);
        gem.StopListening("WinGame", GameOverWin);
    }
    private bool paused;
    private void GameOverLoss()
    {
        gameOverText.text = Constants.GAME_LOST_STRING;
        gameOverBackground.SetActive(true);
        gameOverImageBackground.enabled = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        paused = true;
    }
    private void GameOverWin()
    {
        gameOverText.text = Constants.GAME_WIN_STRING;
        gameOverBackground.SetActive(true);
        gameOverImageBackground.enabled = true;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
        }
        paused = true;
    }
    private void HideMenu()
    {
        Debug.Log("Returning to game...");
        gameOverBackground.SetActive(false);
        gameOverImageBackground.enabled = false;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        paused = false;
    }
    public void PressNewGame()
    {
        gem.TriggerEvent("MenuNewGame");
        HideMenu();
    }
    public void PressQuitToMainMenu()
    {
        gem.TriggerEvent("MenuQuitToMainMenu");
        HideMenu();
    }
    public void PressQuitToDesktop()
    {
        Debug.Log("QuitToDesktop...");
        gem.TriggerEvent("MenuQuitToDesktop");
    }
}
