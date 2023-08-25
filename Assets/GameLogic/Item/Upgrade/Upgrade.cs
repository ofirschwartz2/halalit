using Assets.Enums;
using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    protected virtual void OnPlayerUpgradePickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_UPGRADE_PICKUP, initiator, arguments);
    }
}