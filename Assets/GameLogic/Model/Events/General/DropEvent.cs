using Assets.Enums;
using System;
using System.Collections.Generic;

class DropEvent : Event
{
    private static Dictionary<EventName, EventHandler<DropEventArguments>> _dropEvents;

    public static event EventHandler<DropEventArguments> ItemDrop;

    void Start()
    {
        _dropEvents = new();
        _dropEvents.Add(EventName.ITEM_DROP, ItemDrop);
    }

    public static void Invoke(EventName eventName, object initiator, DropEventArguments upgradeEventArguments)
    {
        Invoke(_dropEvents[eventName], initiator, upgradeEventArguments);
    }
}
