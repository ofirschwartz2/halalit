using Assets.Enums;
using System;
using System.Collections.Generic;

class UpgradeEvent : Event
{
    private static Dictionary<EventName, EventHandler<UpgradeEventArguments>> _upgradeEvents;

    public static event EventHandler<UpgradeEventArguments> PlayerUpgradePickUp;

    void Start()
    {
        _upgradeEvents = new();
        _upgradeEvents.Add(EventName.PLAYER_UPGRADE_PICKUP, PlayerUpgradePickUp);
    }

    public static void Invoke(EventName eventName, object initiator, UpgradeEventArguments upgradeEventArguments)
    {
        Invoke(_upgradeEvents[eventName], initiator, upgradeEventArguments);
    }
}