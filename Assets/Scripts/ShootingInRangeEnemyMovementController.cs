using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class ShootingInRangeEnemyMovementController : MonoBehaviour
{
    public float Force, SpeedLimit;
    public bool UseConfigFile;
    public GameObject ShotPrefab;


    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private Vector3 _direction, _halalitDirection;
    private MoveAimAttackEnemyState _ShootingInRangeEnemyState;
    private float 
        _movingTime, _movingInterval,
        _aimingTime, _aimingInterval,
        _attackingTime, _attackingInterval,
        _shootingRange;
    private bool _didShoot;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _movingInterval = 2;
        _attackingInterval = 1;
        _aimingInterval = 1;
        _shootingRange = 15;
        _didShoot = false;
        _ShootingInRangeEnemyState = MoveAimAttackEnemyState.MOVING;
        _direction = GetMovingDirection();
        _movingTime = GetMovingTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "ShootingInRangeEnemy"; // TODO: should this always be "Enemy" on the Enemy level?
    }

    void Update()
    {

        _halalitDirection = GetHalalitDirection();
        transform.rotation = Quaternion.LookRotation(Vector3.forward, _halalitDirection);

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
            _ShootingInRangeEnemyState = MoveAimAttackEnemyState.MOVING;
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
        Quaternion rotation = GetRandomRotationAround(transform.rotation, _shootingRange);
        Instantiate(ShotPrefab, shootingStartPosition, rotation); 
        _didShoot = true;
    }

    private Quaternion GetRandomRotationAround(Quaternion rotation, float shootingRange)
    {
        float randomAngle = UnityEngine.Random.Range(-shootingRange, shootingRange);
        return rotation * Quaternion.AngleAxis(randomAngle, Vector3.forward);
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

        if (DidAimingTimePass()) 
        {
            _attackingTime = GetAttackingTime();
            _ShootingInRangeEnemyState = MoveAimAttackEnemyState.ATTACKING;
        }

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
        if (IsUnderSpeedLimit())
            _rigidBody.AddForce(_direction * Force);

        if (DidMovingTimePass()) 
        {
            _aimingTime = GetAimingTime();
            _ShootingInRangeEnemyState = MoveAimAttackEnemyState.AIMING;
        }
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
