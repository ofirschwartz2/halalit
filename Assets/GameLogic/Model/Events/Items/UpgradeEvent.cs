using Assets.Enums;
using System;
using System.Collections.Generic;

class UpgradeEvent : Event
{
    private static Dictionary<EventName, EventHandler<UpgradeEventArguments>> _events;

    public static event EventHandler<UpgradeEventArguments> PlayerUpgradePickUp;

    void Start()
    {
        _events = new();
        _events.Add(EventName.PLAYER_UPGRADE_PICKUP, PlayerUpgradePickUp);
    }

    public static void Invoke(EventName eventName, object initiator, UpgradeEventArguments upgradeEventArguments)
    {
        Invoke(_events[eventName], initiator, upgradeEventArguments);
    }
}