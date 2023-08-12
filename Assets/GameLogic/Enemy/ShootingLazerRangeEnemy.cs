using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingLazerRangeEnemy : MonoBehaviour
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
    private float _shotRotationSpeed;
    [SerializeField]
    public GameObject AimingLazerPrefab;
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction, _halalitDirection;
    private MoveAimAttackEnemyState _ShootingInRangeEnemyState;
    private float _movingTime, _aimingTime, _attackingTime;
    private bool _didAim, _didShoot, _didEndShoot;
    private GameObject _aimingShotFrom, _aimingShotTo, _shot;
    private Quaternion shootUpRotationMultiplier = Quaternion.Euler(0f, 0f, 90f); // TODO: fix this BUG
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
            case MoveAimAttackEnemyState.MOVING:
                MovingState();
                break;
            case MoveAimAttackEnemyState.AIMING:
                AimingState();
                break;
            case MoveAimAttackEnemyState.ATTACKING:
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
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _direction, _movementAmplitude, _speedLimit);
        if (DidMovingTimePass())
        {
            SetAiming();
        }
    }

    private void RotateTowardsHalalit() 
    {
        _halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, _halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _shotRotationSpeed);
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
        ShootAimingRays();
        _didAim = true;
    }

    private void ShootAimingRays()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        ShootAimingShotFrom(shootingStartPosition);
        ShootAimingShotTo(shootingStartPosition);
    }

    private void ShootAimingShotFrom(Vector3 shootingStartPosition)
    {
        Quaternion fromRotation = Utils.GetRotation(transform.rotation, -_shootingRange);
        _aimingShotFrom = Instantiate(AimingLazerPrefab, shootingStartPosition, fromRotation * shootUpRotationMultiplier); // shootUpRotationMultiplier BUG
        _aimingShotFrom.transform.SetParent(gameObject.transform);
    }

    private void ShootAimingShotTo(Vector3 shootingStartPosition)
    {
        Quaternion toRotation = Utils.GetRotation(transform.rotation, _shootingRange);
        _aimingShotTo = Instantiate(AimingLazerPrefab, shootingStartPosition, toRotation * shootUpRotationMultiplier); // shootUpRotationMultiplier BUG
        _aimingShotTo.transform.SetParent(gameObject.transform);
    }

    private void AttackingState()
    {
        if (!_didShoot)
        {
            Shoot();
        }
        else if (!_didEndShoot)
        {
            RotateShot();
        }
        else if (DidAttackingTimePass())
        {
            EndAttack();
            SetMoving();
        }
    }

    private void RotateShot()
    {
        _shot.transform.rotation = Quaternion.Slerp(_shot.transform.rotation, _aimingShotTo.transform.rotation, _shotRotationSpeed);
        if (_shot.transform.rotation == _aimingShotTo.transform.rotation) 
        {
            DestroyShot();
        }
    }

    private void DestroyShot()
    {
        _didEndShoot = true;
        Destroy(_shot);
        Destroy(_aimingShotFrom);
        Destroy(_aimingShotTo);
    }
    private void EndAttack()
    {
        _didShoot = false;
        _didEndShoot = false;
    }

    private void SetMoving()
    {
        SetMovingTime();
        _ShootingInRangeEnemyState = MoveAimAttackEnemyState.MOVING;
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
        var shootRotation = _aimingShotFrom.transform.rotation;
        _shot = Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _shot.transform.SetParent(gameObject.transform);
        _didShoot = true;
        _didEndShoot = false;
    }

    private Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude;
        return transform.position + offset;
    }

    private void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _ShootingInRangeEnemyState = MoveAimAttackEnemyState.ATTACKING;
    }

    private bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    private void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
        _ShootingInRangeEnemyState = MoveAimAttackEnemyState.AIMING;
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