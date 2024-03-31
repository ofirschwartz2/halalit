using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DropEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<DropEventArguments>> _dropEvents;

    public static event EventHandler<DropEventArguments> NewDrop;

    void Start()
    {
        _dropEvents = new();
        _dropEvents.Add(EventName.NEW_ITEM_DROP, NewDrop);
        _dropEvents.Add(EventName.NEW_VALUABLE_DROP, NewDrop);
    }

    public static void Invoke(EventName eventName, object initiator, DropEventArguments dropEventArguments)
    {
        Event.Invoke(_dropEvents[eventName], initiator, dropEventArguments);
    }
}
