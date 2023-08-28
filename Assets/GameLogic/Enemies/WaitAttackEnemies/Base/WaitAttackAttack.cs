using UnityEngine;

public abstract class WaitAttackAttack : MonoBehaviour
{
    public abstract void Attack();

    public abstract bool ShouldStopAttacking();

    public abstract void SetAttacking();
}