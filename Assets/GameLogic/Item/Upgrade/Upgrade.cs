using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    protected ItemName _itemName;
    protected Dictionary<EventProperty, float> _pickupEventProperties;

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(_itemName, _pickupEventProperties));
            Destroy(gameObject);
        }
    }

    private void OnPlayerUpgradePickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_UPGRADE_PICKUP, initiator, arguments);
    }
}