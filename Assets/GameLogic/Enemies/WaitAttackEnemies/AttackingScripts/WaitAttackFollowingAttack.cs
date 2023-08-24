using Assets.Utils;
using UnityEngine;

public class WaitAttackFollowingAttack: WaitAttackAttack
{

    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _oneAttackInterval;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private float _stratAttackingTime, _finishAttackingTime;
    private Vector2 _halalitDirection;

    public override void Attack() 
    {
        RotateTowardsHalalit();
        EnemyStraightLineMovement.MoveInStraightLine(_rigidBody, _halalitDirection, _movementAmplitude, _oneAttackInterval, _stratAttackingTime);
    }
    private void RotateTowardsHalalit()
    {
        var halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public override bool ShouldStopAttacking() 
    {
        return Time.time > _finishAttackingTime;
    }

    public override void SetAttacking() 
    {
        SetHalalitDirection();
        SetAttackingTimes();
    }

    private void SetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag("Halalit");
        var halalitPosition = halalit.transform.position;
        _halalitDirection = Utils.NormalizeVector2(halalitPosition - transform.position);
    }

    private void SetAttackingTimes()
    {
        _finishAttackingTime = Time.time + _oneAttackInterval;
        _stratAttackingTime = Time.time;
    }
}