using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class WaitAttackFollowingAttack: WaitAttackAttack
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _oneAttackInterval;
    [SerializeField]
    private float _rotationSpeed;
    
    private float _stratAttackingTime, _finishAttackingTime;
    private Vector2 _halalitDirection;
    private bool _didHit;

    private void Start()
    {
        _didHit = false;
    }

    public override void Attack() 
    {
        RotateTowardsHalalit();
        EnemyMovementUtils.MoveInStraightLine(_rigidBody, _halalitDirection, _movementAmplitude, _oneAttackInterval, _stratAttackingTime);
    }

    private void RotateTowardsHalalit()
    {
        var halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public override bool ShouldStopAttacking() 
    {
        return 
            Time.time > _finishAttackingTime || _didHit;
    }

    public override void SetAttacking() 
    {
        _didHit = false;
        SetHalalitDirection();
        SetAttackingTimes();
    }

    private void SetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var halalitPosition = halalit.transform.position;
        _halalitDirection = Utils.NormalizeVector2(halalitPosition - transform.position);
    }

    private void SetAttackingTimes()
    {
        _finishAttackingTime = Time.time + _oneAttackInterval;
        _stratAttackingTime = Time.time;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            _didHit = true;
        }
    }


}