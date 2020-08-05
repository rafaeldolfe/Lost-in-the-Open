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

        private float signVisibleTime = 1.0f;

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
            gem.StartListening("EndTurn", FlashEndTurnSign);
        }
        void OnDestroy()
        {
            gem.StopListening("EndTurn", FlashEndTurnSign);
        }

        private void FlashEndTurnSign(GameObject invoker, List<object> parameters, int x, int y, int tx, int ty)
        {
            StartCoroutine(FlashEndTurnSign());
        }

        private IEnumerator FlashEndTurnSign()
        {
            sign.SetActive(true);
            signText.SetActive(true);
            yield return new WaitForSeconds(signVisibleTime);
            sign.SetActive(false);
            signText.SetActive(false);
            gem.TriggerEvent("BeginAITurn", gameObject);
        }
    }
}