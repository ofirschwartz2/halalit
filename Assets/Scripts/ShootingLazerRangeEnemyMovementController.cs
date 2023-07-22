using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class ShootingLazerRangeEnemyMovementController : MonoBehaviour
{
    public float Force, SpeedLimit;
    public bool UseConfigFile;
    public GameObject 
        ShotPrefab,
        AimingFromPrefab,
        AimingToPrefab;


    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private Vector3 _direction, _halalitDirection;
    private ShootingInRangeEnemyState _ShootingInRangeEnemyState;
    private float 
        _movingTime, _movingInterval, _speedUpSlowDownInterval,
        _aimingTime, _aimingInterval,
        _attackingTime, _attackingInterval,
        _shootingRange;
    private bool _didShoot, _didShootAiming, _isSpeedingUp;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _movingInterval = 2;
        _speedUpSlowDownInterval = (5 / 10) * _movingInterval;
        _attackingInterval = 1;
        _aimingInterval = 2;
        _shootingRange = 15;
        _didShoot = false;
        _didShootAiming = false;
        _isSpeedingUp = true;
        _ShootingInRangeEnemyState = ShootingInRangeEnemyState.MOVING;
        _direction = GetMovingDirection();
        _movingTime = GetMovingTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "ShootingLazerRangeEnemy"; // TODO: should this always be "Enemy" on the Enemy level?
    }

    void Update()
    {

        _halalitDirection = GetHalalitDirection();
        transform.rotation = Quaternion.LookRotation(Vector3.forward, _halalitDirection);

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

    private void TryChangeSpeedUpSlowDown()
    {
        if (ShouldSlowDown())
        {
            _isSpeedingUp = false;
        }
        else
        {
            _isSpeedingUp = true;
        }
    }

    private bool ShouldSlowDown()
    {
        if (_movingTime - Time.time < _speedUpSlowDownInterval) 
        {
            Debug.Log("ShouldSlowDown");
        }
        return _movingTime - Time.time < _speedUpSlowDownInterval;
    }

    private void AttackingState()
    {
        if (!_didShoot) 
        {
            Shoot();
        }
        
        else if (DidAttackingTimePass())
        {
            _didShoot = false;
            _movingTime = GetMovingTime();
            _ShootingInRangeEnemyState = ShootingInRangeEnemyState.MOVING;
        }
    }

    private bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    private float GetAttackingTime()
    {
        return Time.time + _attackingInterval;
    }


    private void Shoot()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        Quaternion fromRotation = GetFromRotation(transform.rotation, -_shootingRange);
        Instantiate(ShotPrefab, shootingStartPosition, fromRotation);
        _didShoot = true;
    }

    private Quaternion GetFromRotation(Quaternion rotation, float shootingRange)
    {
        float fromAngle = -shootingRange;
        return rotation * Quaternion.AngleAxis(fromAngle, Vector3.forward);
    }
    private Quaternion GetToRotation(Quaternion rotation, float shootingRange)
    {
        float fromAngle = shootingRange;
        return rotation * Quaternion.AngleAxis(fromAngle, Vector3.forward);
    }

    private Vector3 GetShootingStartPosition()
    {
        Vector3 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _halalitDirection.normalized * halfExtents.magnitude;
        return transform.position + offset;
    }
    private Vector2 GetMovingDirection()
    {
        return new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f));
    }

    private void AimingState()
    {
        if (!_didShootAiming) 
        {
            ShootAimingRays();
            _didShootAiming = true;
        }

        if (DidAimingTimePass()) 
        {
            _attackingTime = GetAttackingTime();
            _didShootAiming = false;
            _ShootingInRangeEnemyState = ShootingInRangeEnemyState.ATTACKING;
        }
    }

    private void ShootAimingRays()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        Quaternion fromRotation = GetFromRotation(transform.rotation, -_shootingRange);
        Quaternion toRotation = GetFromRotation(transform.rotation, _shootingRange);
        Instantiate(AimingFromPrefab, shootingStartPosition, fromRotation);
        Instantiate(AimingToPrefab, shootingStartPosition, toRotation);
    }
    private bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    private Vector3 GetHalalitDirection()
    {
        var halalit = GameObject.FindGameObjectWithTag("Halalit");
        var halalitPosition = halalit.transform.position;
        return halalitPosition - transform.position;
    }

    private Vector2 NormalizeVector2(Vector2 vector) // TODO: MOVE TO UTILS
    {
        var magnitude = (float)Math.Sqrt(Math.Pow(vector.x, 2) + Math.Pow(vector.y, 2));
        return new Vector2(vector.x / magnitude, vector.y / magnitude);
    }

    private void MovingState()
    {
        TryChangeSpeedUpSlowDown();

        if (IsUnderSpeedLimit()) 
        {
            _rigidBody.AddForce(_direction * Force * GetSpeedUpDlowDownNumber());
        }

        if (DidMovingTimePass()) 
        {
            _aimingTime = GetAimingTime();
            _ShootingInRangeEnemyState = ShootingInRangeEnemyState.AIMING;
        }
    }

    private float GetSpeedUpDlowDownNumber()
    {
        return _isSpeedingUp ? 1 : -1;
    }

    private float GetAimingTime()
    {
        return Time.time + _aimingInterval;
    }

    private bool DidMovingTimePass()
    {
        return Time.time > _movingTime;
    }

    private float GetMovingTime()
    {
        return Time.time + _movingInterval;
    }

    private bool IsUnderSpeedLimit() // TODO: MOVE TO Enemy UTILS / UTILS
    {
        return Utils.GetVectorMagnitude(_rigidBody.velocity) < SpeedLimit;
    }

    private void ConfigureFromFile()
    {
        string[] props = { "Force", "SpeedLimit" };
        Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);
        Force = propsFromConfig["Force"];
        SpeedLimit = propsFromConfig["SpeedLimit"];
    }
}
