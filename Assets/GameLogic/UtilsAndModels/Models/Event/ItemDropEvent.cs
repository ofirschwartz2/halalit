using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<ItemDropEventArguments>> _itemDropEvents;

    public static event EventHandler<ItemDropEventArguments> NewItemDrop;

    void Start()
    {
        _itemDropEvents = new();
        _itemDropEvents.Add(EventName.NEW_ITEM_DROP, NewItemDrop);
    }

    public static void Invoke(EventName eventName, object initiator, ItemDropEventArguments dropEventArguments)
    {
        Event.Invoke(_itemDropEvents[eventName], initiator, dropEventArguments);
    }
}
