using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using Encounter;

public class EventLoopManager : MonoBehaviour
{
    private bool interrupted;
    private LinkedList<Event> events = new LinkedList<Event>();
    private IEnumerator currentEvent;


    private void Start()
    {
        StartCoroutine(Loop());
    }
    private Event Dequeue()
    {
        Event first = events.First.Value;
        events.RemoveFirst();
        return first;
    }

    public IEnumerator Loop()
    {
        while(true)
        {
            if(events.Count > 0)
            {
                Event first = Dequeue();
                Decision dec = first.decision;
                yield return dec.ability.UseAbility(dec.path);
            }
            else
            {
                yield return null;
            }
        }
    }

    public void AddEvent(Decision decision)
    {
        Event ev = new Event(decision);
        events.AddLast(ev);
    }

    public void AddHardInterrupt(Decision decision)
    {
        interrupted = true;
        events.Clear();
        Event ev = new Event(decision);
        events.AddLast(ev);
    }

    public void CancelEvents(List<Ability> abilities)
    {
        events = new LinkedList<Event>(events.Where(ev => !abilities.Contains(ev.decision.ability)).ToList());
    }
    public IEnumerator WaitForInterrupt()
    {
        return new WaitUntil(() => {
            bool tmp = interrupted;
            interrupted = false;
            return tmp;
        });
    }
    public IEnumerator WaitForEvents()
    {
        return new WaitUntil(() => events.Count == 0);
    }

    internal void InterruptionHandled()
    {
        interrupted = false;
    }

    internal bool WasInterrupted()
    {
        return interrupted;
    }

    internal bool EventsFinished()
    {
        return events.Count == 0;
    }
}

public class Event
{
    public Decision decision;

    public Event(Decision decision)
    {
        this.decision = decision;
    }
}
