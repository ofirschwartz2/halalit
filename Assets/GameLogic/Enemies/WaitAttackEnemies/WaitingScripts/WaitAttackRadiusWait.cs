using Assets.Utils;
using UnityEngine;

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
        var halalitDirection = EnemyTarget.GetDirection(transform);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }
    
    public override bool ShouldStopWaiting()
    {
        return DidWaitingTimePass() && IsHalalitInRadius();
    }

    private bool DidWaitingTimePass()
    {
        return Time.time > _waitingTime;
    }

    private bool IsHalalitInRadius()
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, EnemyTarget.GetPosition(transform)) < _waitingRadius;
    }

    // The charge waits for the Halalit to come within the radius, and nobody knows when that will be.
    // So the wind-up runs from the last seconds of the wait until the charge.
    public override bool IsWindingUp(float seconds)
    {
        return Time.time > _waitingTime - seconds;
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