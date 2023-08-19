using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingLazerRangeAttack : MoveAimAttackAttack
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
    private float _shotRotationSpeed;
    [SerializeField]
    public GameObject AimShotPrefab;
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction, _halalitDirection;
    private MoveAimAttackEnemyState _shootingLazerRangeEnemyState;
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
        _shootingLazerRangeEnemyState = MoveAimAttackEnemyState.ATTACKING;
    }
}