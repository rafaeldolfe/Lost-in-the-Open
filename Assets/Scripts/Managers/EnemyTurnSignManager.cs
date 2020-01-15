using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyTurnSignManager : MonoBehaviour
{
    private GlobalEventManager gem;

    public GameObject sign;
    public GameObject signText;

    private float signVisibleTime = 1.0f;

    void Awake()
    {
        List<Type> depTypes = ProgramUtils.GetMonoBehavioursOnType(this.GetType());
        List<MonoBehaviour> deps = new List<MonoBehaviour>();

        deps.Add(gem = FindObjectOfType(typeof(GlobalEventManager)) as GlobalEventManager);
        if (deps.Contains(null))
        {
            throw ProgramUtils.DependencyException(deps, depTypes);
        }
    }

    void Start()
    {
        gem.StartListening("EndTurn", FlashEndTurnSign);
    }
    void OnDestroy()
    {
        gem.StopListening("EndTurn", FlashEndTurnSign);
    }

    private void FlashEndTurnSign(GameObject invoker, List<object> parameters, int x, int z, int tx, int tz)
    {
        Debug.Log("Flashing!!");
        StartCoroutine(FlashEndTurnSign());
    }

    private IEnumerator FlashEndTurnSign()
    {
        Debug.Log("Flashing!!");
        sign.SetActive(true);
        signText.SetActive(true);
        yield return new WaitForSeconds(signVisibleTime);
        sign.SetActive(false);
        signText.SetActive(false);
        gem.TriggerEvent("BeginAITurn", gameObject);
    }
}
