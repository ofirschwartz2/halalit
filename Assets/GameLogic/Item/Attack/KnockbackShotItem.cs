using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class KnockbackShotItem : AttackItem
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(ItemName.KNOCKBACK_SHOT, new()));
            Destroy(gameObject);
        }
    }
}
