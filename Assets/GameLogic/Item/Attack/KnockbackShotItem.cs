using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class KnockbackShotItem : AttackItem
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(ItemName.LASER_BEAM, new()));
            Destroy(gameObject);
        }
    }
}
