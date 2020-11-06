using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Utils;

namespace Encounter
{
    public class EnemyTurnSignManager : MonoBehaviour
    {
        private GlobalEventManager gem;
        private CameraManager cm;

        public GameObject sign;
        public GameObject signText;

        private float signVisibleTime = 0.0f;

        void Awake()
        {
            List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
            List<MonoBehaviour> deps = new List<MonoBehaviour>
        {
            (gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager),
            (cm = FindObjectOfType(typeof(CameraManager)) as CameraManager),
        };
            if (deps.Contains(null))
            {
                throw ProgramUtils.DependencyException(deps, depTypes);
            }
            cm.AttachToCamera(gameObject);
            gem.StartListening("PlayerEndTurn", FlashEndTurnSign);
        }
        void OnDestroy()
        {
            gem.StopListening("PlayerEndTurn", FlashEndTurnSign);
        }

        private void FlashEndTurnSign()
        {
            StartCoroutine(ShowEndTurnSign());
        }

        private IEnumerator ShowEndTurnSign()
        {
            if (signVisibleTime < 0.1f)
            {
                gem.TriggerEvent("EnemyBeginTurn", gameObject);
                yield break;
            }
            sign.SetActive(true);
            signText.SetActive(true);
            yield return new WaitForSeconds(signVisibleTime);
            sign.SetActive(false);
            signText.SetActive(false);
            gem.TriggerEvent("EnemyBeginTurn", gameObject);
        }
    }
}