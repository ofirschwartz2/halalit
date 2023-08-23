using Assets.Enums;
using UnityEngine;

public abstract class AttackItem : MonoBehaviour
{
    protected virtual void OnPlayerAttackItemPickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_ATTACK_ITEM_PICKUP, initiator, arguments);
    }
}
