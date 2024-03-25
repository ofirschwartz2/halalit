using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<ItemEventArguments>> _itemEvents;

    public static event EventHandler<ItemEventArguments> PlayerUpgradePickUp;
    public static event EventHandler<ItemEventArguments> PlayerAttackItemPickUp;

    void Start()
    {
        _itemEvents = new();
        _itemEvents.Add(EventName.PLAYER_UPGRADE_PICKUP, PlayerUpgradePickUp);
        _itemEvents.Add(EventName.PLAYER_ATTACK_ITEM_PICKUP, PlayerAttackItemPickUp);
    }

    public static void Invoke(EventName eventName, object initiator, ItemEventArguments itemEventArguments)
    {
        Event.Invoke(_itemEvents[eventName], initiator, itemEventArguments);
    }
}