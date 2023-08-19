
using UnityEngine;

public abstract class MoveAimAttackAim : MonoBehaviour
{
    [SerializeField]
    private float _aimingInterval;

    private float _aimingTime;
    private bool _didAim;

    public void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
    }

    public bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    public void AimingState(Transform transform)
    {
        if (!_didAim)
        {
            Aim(transform);
            _didAim = true;
        }
    }

    public abstract void Aim(Transform transform);
}