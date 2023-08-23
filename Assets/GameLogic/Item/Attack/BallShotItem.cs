using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class BallShotItem : AttackItem
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(ItemName.BALL_SHOT, new()));
            Destroy(gameObject);
        }
    }
}
