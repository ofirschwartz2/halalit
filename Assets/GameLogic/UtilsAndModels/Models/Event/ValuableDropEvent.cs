using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ValuableDropEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<ValuableDropEventArguments>> _dropEvents;

    public static event EventHandler<ValuableDropEventArguments> NewValuableDrop;

    void Start()
    {
        _dropEvents = new();
        _dropEvents.Add(EventName.NEW_VALUABLE_DROP, NewValuableDrop);
    }

    public static void Invoke(EventName eventName, object initiator, ValuableDropEventArguments dropEventArguments)
    {
        Event.Invoke(_dropEvents[eventName], initiator, dropEventArguments);
    }
}
