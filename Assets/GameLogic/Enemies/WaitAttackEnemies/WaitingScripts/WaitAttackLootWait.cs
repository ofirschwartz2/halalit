using Assets.Utils;
using UnityEngine;

// Coin Thief: drifts slowly in a seeded direction, and stops waiting once the wait is over and a free valuable is within its loot radius.
public class WaitAttackLootWait : WaitAttackWait
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private EnemySharedBehavior _enemySharedBehavior;
    [SerializeField]
    private WaitAttackStealAttack _stealAttack;
    [SerializeField]
    private float _waitingInterval = 2f;
    [SerializeField]
    private float _driftSpeed = 0.5f;
    [SerializeField]
    private float _movementAmplitude = 10f;

    private float _waitingTime;
    private Vector2 _driftDirection;

    public override void Wait()
    {
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _driftDirection, _movementAmplitude, _driftSpeed);
    }

    public override bool ShouldStopWaiting()
    {
        return DidWaitingTimePass() && _stealAttack.IsLootInReach();
    }

    public override void SetWaiting()
    {
        _waitingTime = Time.time + _waitingInterval;
        _driftDirection = Utils.GetRandomVector2OnCircle(_enemySharedBehavior._seedfulRandomGenerator);
    }

    private bool DidWaitingTimePass()
    {
        return Time.time > _waitingTime;
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            _driftDirection = EnemyUtils.GetAnotherDirectionFromEdge(_enemySharedBehavior._seedfulRandomGenerator, _rigidBody, other.gameObject.tag);
        }
    }
    #endregion

#if UNITY_EDITOR
    internal float GetWaitingInterval()
    {
        return _waitingInterval;
    }
#endif
}
