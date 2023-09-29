using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public abstract class AttackItem : MonoBehaviour
{
    protected ItemName _itemName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(_itemName, new()));
            Destroy(gameObject);
        }
    }

    private void OnPlayerAttackItemPickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_ATTACK_ITEM_PICKUP, initiator, arguments);
    }
}
