using Assets.Enums;
public class MirrorBallShotItem : AttackItem
{
    private void Start()
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerAttackItemPickedUp(this, new(ItemName.MIRROR_BALL_SHOT, new()));
            Destroy(gameObject);
        }
    }
}
