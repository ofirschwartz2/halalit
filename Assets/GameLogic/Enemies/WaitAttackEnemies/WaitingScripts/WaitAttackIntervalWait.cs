using UnityEngine;

public class WaitAttackIntervalWait : WaitAttackWait
{

    [SerializeField]
    private float _waitingInterval;
    [SerializeField]
    private float _rotationSpeed;

    private float _waitingTime;

    public override void Wait() 
    {
        Rotate();
    }

    private void Rotate()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * Quaternion.Euler(0, 0, 180), _rotationSpeed * Time.deltaTime);
    }

    public override bool ShouldStopWaiting()
    {
        return Time.time > _waitingTime;
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