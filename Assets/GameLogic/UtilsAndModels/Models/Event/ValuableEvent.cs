using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ValuableEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<ValuableEventArguments>> _valuableEvents;

    public static event EventHandler<ValuableEventArguments> PlayerValuablePickUp;

    void Start()
    {
        _valuableEvents = new();
        _valuableEvents.Add(EventName.PLAYER_VALUABLE_PICKUP, PlayerValuablePickUp);
    }

    public static void Invoke(EventName eventName, object initiator, ValuableEventArguments valuableEventArguments)
    {
        Event.Invoke(_valuableEvents[eventName], initiator, valuableEventArguments);
    }
}