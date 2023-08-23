using Assets.Utils;
using UnityEngine;
using UnityEngine.UIElements;

public class WaitAttackRadiusWait : WaitAttackWait
{

    [SerializeField]
    private float _waitingRadius;
    [SerializeField]
    private float _waitingInterval;
    [SerializeField]
    private float _rotationSpeed;
    private float _waitingTime;

    public override void Wait() 
    {
        RotateTowardsHalalit();
    }

    private void RotateTowardsHalalit()
    {
        var halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
    
    public override bool ShouldStopWaiting()
    {
        return
            DidWaitingTimePass()
            &&
            IsHalalitInRadius();
    }

    private bool DidWaitingTimePass()
    {
        return Time.time > _waitingTime;
    }

    private bool IsHalalitInRadius()
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, Utils.GetHalalitPosition()) < _waitingRadius;
    }

    public override void SetWaiting()
    {
        SetWaitingTime();
    }

    private void SetWaitingTime()
    {
        _waitingTime = Time.time + _waitingInterval;
    }
}