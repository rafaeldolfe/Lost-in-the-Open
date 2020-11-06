using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using Utils;

namespace Encounter
{
    public class EndTurnManager : MonoBehaviour
    {
        private GlobalEventManager gem;

        public GameObject endTurnButton;
        public Image endTurnImage;
        public TextMeshProUGUI buttonText;
        public Sprite buttonOn;
        public Sprite buttonOff;

        private const float transparencyCoeff = 0.7f;
        private Color originalColor;
        private Color transparentVersion;
        private bool turnDone = false;
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

            originalColor = endTurnImage.color;
            transparentVersion = endTurnImage.color;
            transparentVersion.a = transparentVersion.a - transparencyCoeff;

            gem.StartListening("OfferEndTurn", OfferEndTurnButtonHandler);
            gem.StartListening("PlayerEndTurn", EndTurnHandler);
        }
        void Start()
        {
            HideEndTurnButton();
        }
        void OnDestroy()
        {
            gem.StopListening("OfferEndTurn", OfferEndTurnButtonHandler);
            gem.StopListening("PlayerEndTurn", EndTurnHandler);
        }
        public void ChangeToButtonReleasedSprite()
        {
            endTurnImage.sprite = buttonOn;
        }
        public void ChangeToButtonPressedSprite()
        {
            endTurnImage.sprite = buttonOff;
        }
        public void PressButton()
        {
            gem.TriggerEvent("PlayerEndTurn", gameObject);
        }
        public void OfferEndTurnButtonHandler(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            turnDone = true;
            ShowEndTurnButton();
        }
        public void EndTurnHandler()
        {
            turnDone = false;
        }
        public void HideEndTurnButton()
        {
            if (!turnDone)
            {
                endTurnImage.color = transparentVersion;
                buttonText.color = transparentVersion;
            }
        }
        public void ShowEndTurnButton()
        {
            endTurnImage.color = originalColor;
            buttonText.color = originalColor;
        }
    }
}