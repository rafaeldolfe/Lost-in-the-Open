using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class EndTurnManager : MonoBehaviour
{
    private GlobalEventManager gem;
    private CameraManager cm;
    private FactionManager fm;

    public GameObject endTurnButton;
    public SpriteRenderer endTurnSprite;
    public TextMeshPro buttonText;
    public Sprite buttonOn;
    public Sprite buttonOff;

    private const float transparencyCoeff = 0.7f;
    private Color originalColor;
    private Color transparentVersion;
    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        deps.Add(cm = FindObjectOfType(typeof(CameraManager)) as CameraManager);
        deps.Add(fm = FindObjectOfType(typeof(FactionManager)) as FactionManager);
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }

        originalColor = endTurnSprite.material.color;
        transparentVersion = endTurnSprite.material.color;
        transparentVersion.a = transparentVersion.a - transparencyCoeff;
    }
    void Start()
    {
        gem.StartListening("OfferEndTurn", OfferEndTurnButtonHandler);
        gem.StartListening("EndTurn", EndTurnHandler);
        cm.AttachToCamera(endTurnButton);
        HideEndTurnButton();
    }
    void OnDestroy()
    {
        gem.StopListening("OfferEndTurn", OfferEndTurnButtonHandler);
        gem.StopListening("EndTurn", EndTurnHandler);
    }
    void OnMouseEnter()
    {
        ShowEndTurnButton();
    }
    void OnMouseExit()
    {
        ChangeToButtonReleasedSprite();
        HideEndTurnButton();
    }
    void OnMouseDown()
    {
        ChangeToButtonPressedSprite();
        gem.TriggerEvent("EndTurn", gameObject);
    }
    void OnMouseUp()
    {
        ChangeToButtonReleasedSprite();
    }
    private void ChangeToButtonReleasedSprite()
    {
        endTurnSprite.sprite = buttonOn;
    }
    private void ChangeToButtonPressedSprite()
    {
        endTurnSprite.sprite = buttonOff;
    }

    private void OfferEndTurnButtonHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        turnDone = true;
        ShowEndTurnButton();
    }
    private void EndTurnHandler(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        turnDone = false;
    }

    private bool turnDone = false;
    private void HideEndTurnButton()
    {
        if (!turnDone)
        {
            endTurnSprite.material.color = transparentVersion;
            buttonText.color = transparentVersion;
        }
    }
    private void ShowEndTurnButton()
    {
        endTurnSprite.material.color = originalColor;
        buttonText.color = originalColor;
    }
}
