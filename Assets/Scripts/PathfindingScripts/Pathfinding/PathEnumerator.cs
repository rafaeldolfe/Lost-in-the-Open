using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Encounter;

public class PathEnumerator : IEnumerator
{
    public IEnumerator Coroutine;
    private List<PathNode> Preset;
    public PathEnumerator(IEnumerator Coroutine)
    {
        this.Coroutine = Coroutine;
    }

    public PathEnumerator(List<PathNode> path)
    {
        Coroutine = null;
        Preset = path;
    }

    public List<PathNode> Result
    {
        get
        {
            if (Coroutine == null && Preset != null)
            {
                return Preset;
            }
            else if (Coroutine == null)
            {
                throw new System.Exception("Invalid fields, Coroutine is null without preset");
            }
            if (Current == null)
            {
                return null;
            }
            else if (Current.GetType() != typeof(PathfindingResolution))
            {
                throw new System.Exception($"Invalid value; PathEnumerator returned something other than PathfindingResolution: {Current.GetType()}");
            }
            PathfindingResolution pathr = (PathfindingResolution) Current;

            if (pathr.Result != ThreadedPathfinding.PathfindingResult.SUCCESSFUL)
            {
                Debug.Log($"PathEnumerator tried to return failed PathfindingResolution:");
                Debug.Log($"res {pathr.Id} with path length {pathr.Path?.Count} failed with result {pathr.Result}");
                Debug.LogWarning($"{pathr.Result}");
            }

            return pathr.Path;
        }
    }

    public object Current => Coroutine.Current;

    public bool MoveNext()
    {
        return Coroutine.MoveNext();
    }

    public void Reset()
    {
        Coroutine.Reset();
    }
}

public class DecisionsEnumerator : IEnumerator
{
    public IEnumerator Coroutine;

    public DecisionsEnumerator(IEnumerator Coroutine)
    {
        this.Coroutine = Coroutine;
    }

    public List<Decision> Result
    {
        get
        {
            if (Current == null)
            {
                return null;
            }
            else if (Current.GetType() != typeof(List<Decision>))
            {
                return null;
            }
            else
            {
                List<Decision> decisions = (List<Decision>)Current;
                return decisions;
            }
        }
    }

    public object Current => Coroutine.Current;

    public bool MoveNext()
    {
        return Coroutine.MoveNext();
    }

    public void Reset()
    {
        Coroutine.Reset();
    }
}
public class AnalysisEnumerator : IEnumerator
{
    public IEnumerator Coroutine;

    public AnalysisEnumerator(IEnumerator Coroutine)
    {
        this.Coroutine = Coroutine;
    }

    public Analysis Result
    {
        get
        {
            if (Current == null)
            {
                return null;
            }
            else if (Current.GetType() != typeof(Analysis))
            {
                return null;
            }
            else
            {
                Analysis decisions = (Analysis)Current;
                return decisions;
            }
        }
    }

    public object Current => Coroutine.Current;

    public bool MoveNext()
    {
        return Coroutine.MoveNext();
    }

    public void Reset()
    {
        Coroutine.Reset();
    }
}