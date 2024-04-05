using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsBankEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<ItemsBankEventArguments>> _itemsBankEvents;

    public static event EventHandler<ItemsBankEventArguments> NoStock;

    void Start()
    {
        _itemsBankEvents = new();
        _itemsBankEvents.Add(EventName.NO_STOCK, NoStock);
    }

    public static void Invoke(EventName eventName, object initiator, ItemsBankEventArguments itemsBankEventArguments)
    {
        Event.Invoke(_itemsBankEvents[eventName], initiator, itemsBankEventArguments);
    }
}

