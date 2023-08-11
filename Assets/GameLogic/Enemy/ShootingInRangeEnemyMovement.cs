using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingInRangeEnemyMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _movingInterval;
    [SerializeField]
    private float _aimingInterval;
    [SerializeField] 
    private float _attackingInterval;
    [SerializeField]
    private float _shootingRange;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction, _halalitDirection;
    private ShootingInRangeEnemyState _ShootingInRangeEnemyState;
    private float _movingTime, _aimingTime, _attackingTime, _shootAngle;
    private bool _didShoot, _didAim;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetDirection();
        SetMoving();
    }

    void Update()
    {
        switch (_ShootingInRangeEnemyState)
        {
            case ShootingInRangeEnemyState.MOVING:
                MovingState();
                break;
            case ShootingInRangeEnemyState.AIMING:
                AimingState();
                break;
            case ShootingInRangeEnemyState.ATTACKING:
                AttackingState();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void SetDirection()
    {
        _direction = Utils.GetRandomVector2OnCircle();
    }

    private void MovingState()
    {
        RotateTowardsHalalit();
        if (Utils.IsUnderSpeedLimit(_rigidBody.velocity, _speedLimit))
        {
            _rigidBody.AddForce(_direction * _movementAmplitude * (Time.deltaTime * 300));
        }

        if (DidMovingTimePass())
        {
            SetAiming();
        }
    }

    private void RotateTowardsHalalit() 
    {
        _halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, _halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed);
    }

    private void AimingState()
    {
        if (!_didAim) 
        {
            Aim();
        }
        if (DidAimingTimePass())
        {
            SetAttacking();
        }
    }

    private void Aim()
    {
        _shootAngle = Utils.GetRandomAngleAround(_shootingRange);
        _didAim = true;
    }

    private void AttackingState()
    {
        if (!_didShoot)
        {
            Shoot();
        }

        else if (DidAttackingTimePass())
        {
            SetMoving();
        }
    }

    private void SetMoving()
    {
        _didShoot = false;
        SetMovingTime();
        _ShootingInRangeEnemyState = ShootingInRangeEnemyState.MOVING;
    }

    private void SetMovingTime()
    {
        _movingTime = Time.time + _movingInterval;
    }

    private bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    private void Shoot()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        var shootRotation = Utils.GetRotation(transform.rotation, _shootAngle);
        Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _didShoot = true;
    }

    private Vector3 GetShootingStartPosition()
    {
        Vector3 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _halalitDirection.normalized * halfExtents.magnitude;
        return transform.position + offset;
    }

    private void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _ShootingInRangeEnemyState = ShootingInRangeEnemyState.ATTACKING;
    }

    private bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    private void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
        _ShootingInRangeEnemyState = ShootingInRangeEnemyState.AIMING;
    }

    private bool DidMovingTimePass()
    {
        return Time.time > _movingTime;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void SetNewDirection(string edgeTag)
    {
        _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, edgeTag);
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: refactor this. should this be in the EventHandler?
        if (EnemyUtils.ShouldKillMe(other))
        {
            Die();
        }
        else if (EnemyUtils.ShouldKnockMeBack(other))
        {
            EnemyUtils.KnockMeBack(_rigidBody, other);
        }
        else if (Utils.DidHitEdge(other.gameObject.tag))
        {
            SetNewDirection(other.gameObject.tag);
        }
    }
    #endregion
}