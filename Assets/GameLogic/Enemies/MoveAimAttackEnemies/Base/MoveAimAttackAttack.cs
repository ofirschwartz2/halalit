
using UnityEngine;

public abstract class MoveAimAttackAttack : MonoBehaviour
{
    [SerializeField]
    private float _attackingInterval;

    protected float _attackingTime;
    protected bool _didShoot;

    public void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _didShoot = false;
    }

    public bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    public virtual void AttackingState(Transform transform)
    {
        if (!_didShoot)
        {
            Shoot(transform);
            _didShoot = true;
        }
    }

    public abstract void Shoot(Transform transform);
}