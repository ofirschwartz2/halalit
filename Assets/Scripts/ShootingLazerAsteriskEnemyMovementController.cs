using Assets.Common;
using Assets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using static UnityEngine.GraphicsBuffer;

public class ShootingLazerAsteriskEnemyMovementController : MonoBehaviour
{
    public float Force, SpeedLimit;
    public bool UseConfigFile;
    public GameObject
        AimingPrefab,
        ShotPrefab;


    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private Vector3 _movingDirection;
    private MoveAimAttackEnemyState _ShootingInRangeEnemyState;
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
        _didShoot = false;
        _didShootAiming = false;
        _isSpeedingUp = true;
        _ShootingInRangeEnemyState = MoveAimAttackEnemyState.MOVING;
        _movingDirection = GetMovingDirection();
        _movingTime = GetMovingTime();
        if (UseConfigFile)
            ConfigureFromFile();
        tag = "ShootingLazerAsteriskEnemy"; // TODO: should this always be "Enemy" on the Enemy level?
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
        
        //shoot in all 8 directions: up, up-right, right, right-down, down, left-down, left, left-up
        // every shot is shot from the edge of the enemy: up from the top edge, right from the right edge, etc.
        var rotations = GetAllRotations(transform.rotation);
        var shootingStartPositions = GetShootingStartPositions();
        for (var i = 0; i < shootingStartPositions.Count; i++)
        {
            Instantiate(ShotPrefab, shootingStartPositions[i], rotations[i]);
            //shot.GetComponent<Rigidbody2D>().AddForce(shot.transform.up * Force);
        }
        _didShoot = true;
    }

    // return all the rotations for the 8 directions. the enemy is always moving so the shooting direction is always changing
    private List<Quaternion> GetAllRotations(Quaternion rotation)
    {
        return new List<Quaternion>()
        {
            rotation * Quaternion.identity,
            rotation * Quaternion.Euler(0, 0, 45),
            rotation * Quaternion.Euler(0, 0, 90),
            rotation * Quaternion.Euler(0, 0, 135),
            rotation * Quaternion.Euler(0, 0, 180),
            rotation * Quaternion.Euler(0, 0, 225),
            rotation * Quaternion.Euler(0, 0, 270),
            rotation * Quaternion.Euler(0, 0, 315)
        };
    }

    // return the shooting start position for each of the 8 directions, based on the enemy's current position
    private List<Vector3> GetShootingStartPositions()
    {
        return new List<Vector3>()
        {
            new Vector3(transform.position.x, transform.position.y + 1.1f, transform.position.z),
            new Vector3(transform.position.x - 1.1f, transform.position.y + 1.1f, transform.position.z),
            new Vector3(transform.position.x - 1.1f, transform.position.y, transform.position.z),
            new Vector3(transform.position.x - 1.1f, transform.position.y - 1.1f, transform.position.z),
            new Vector3(transform.position.x, transform.position.y - 1.1f, transform.position.z),
            new Vector3(transform.position.x + 1.1f, transform.position.y - 1.1f, transform.position.z),
            new Vector3(transform.position.x + 1.1f, transform.position.y, transform.position.z),
            new Vector3(transform.position.x + 1.1f, transform.position.y + 1.1f, transform.position.z)
            /*
            new Vector3(transform.position.x + 1.1f, transform.position.y + 1.1f, transform.position.z),
            new Vector3(transform.position.x + 1.1f, transform.position.y, transform.position.z),
            new Vector3(transform.position.x + 1.1f, transform.position.y - 1.1f, transform.position.z),
            new Vector3(transform.position.x, transform.position.y - 1.1f, transform.position.z),
            
            
            
            */        
            };
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
            _ShootingInRangeEnemyState = MoveAimAttackEnemyState.ATTACKING;
        }
    }

    private void ShootAimingRays()
    {
        //Vector3 shootingStartPosition = GetShootingStartPositions();
        //Quaternion upRotation = GetFromRotation(transform.rotation, -_shootingRange);
        //Quaternion toRotation = GetFromRotation(transform.rotation, _shootingRange);
        //Instantiate(AimingPrefab, shootingStartPosition, fromRotation);
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
            _rigidBody.AddForce(_movingDirection * Force * GetSpeedUpDlowDownNumber());
        }

        if (DidMovingTimePass()) 
        {
            _aimingTime = GetAimingTime();
            _ShootingInRangeEnemyState = MoveAimAttackEnemyState.AIMING;
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
