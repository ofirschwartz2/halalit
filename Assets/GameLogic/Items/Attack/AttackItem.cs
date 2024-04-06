using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public abstract class AttackItem : MonoBehaviour
{
    public ItemStats AttackStats;

    protected ItemName _itemName;

    private void OnTriggerEnter2D(Collider2D other) // TODO: Move to a Collectable Common class?
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(_itemName, AttackStats.itemStats));
            Destroy(gameObject);
        }
    }

    protected void OnPlayerAttackItemPickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_ATTACK_ITEM_PICKUP, initiator, arguments);
    }
}
