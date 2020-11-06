using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThreadedPathfinding;
using System;

public class ValueCoroutine<T>
{
    public Coroutine Coroutine { get; private set; }
    public T result;
    private readonly IEnumerator target;
    public ValueCoroutine(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.Coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            Debug.Log($"result before: {(result == null ? "null" : result.ToString())}");

            try
            {
                result = (T)target.Current;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }

            Debug.Log($"result after: {(result == null ? "null" : result.ToString())}");
            yield return result;
        }

        //Maybe warn the developer that he or she might have messed up the choice of T

        Debug.Log($"target.MoveNext() gave false");

        Debug.Log($"this is target.Current: {target.Current}");
    }
}
