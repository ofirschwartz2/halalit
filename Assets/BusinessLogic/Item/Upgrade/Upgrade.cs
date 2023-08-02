using Assets.Enums;
using UnityEngine;

public abstract class Upgrade: MonoBehaviour
{
    protected virtual void OnPlayerUpgradePickedUp(object initiator, UpgradeEventArguments arguments)
    {
        UpgradeEvent.Invoke(EventName.PLAYER_UPGRADE_PICKUP, initiator, arguments);
    }
}