using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ShootingLazerAsteriskEnemy : MonoBehaviour
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
    private float _rotationSpeed;
    [SerializeField]
    private float _numberOfShots; // TODO: INT
    [SerializeField]
    public GameObject AimShotPrefab;
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction, _halalitDirection;
    private MoveAimAttackEnemyState _shootingLazerAsteriskEnemyState;
    private float _movingTime, _aimingTime, _attackingTime;
    private bool _didAim, _didShoot;
    private List<GameObject> _aimingShots, _shots;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _aimingShots = new List<GameObject>();
        _shots = new List<GameObject>();

        SetDirection();
        SetMoving();
    }

    void Update()
    {
        switch (_shootingLazerAsteriskEnemyState)
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
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _direction, _movementAmplitude, _speedLimit);

        if (DidMovingTimePass())
        {
            SetAiming();
        }
    }

    private void AimingState()
    {
        if (!_didAim) 
        {
            Aim();
        }
        else if (DidAimingTimePass())
        {
            EndAiming();
            SetAttacking();
        }
    }

    private void Aim()
    {
        ShootRays(AimShotPrefab, _aimingShots);
        _didAim = true;
    }

    private void EndAiming()
    {
        _didAim = false;
        DestroyShots(_aimingShots);

    }

    private void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _shootingLazerAsteriskEnemyState = MoveAimAttackEnemyState.ATTACKING;
    }

    private void ShootRays(GameObject shotPrefab, List<GameObject> shotsList)
    {
        var aimingStartPositions = GetShootingStartPositions((int)_numberOfShots, GetComponent<CircleCollider2D>().radius);
        foreach (var aimingStartPosition in aimingStartPositions)
        {
            var shot = Instantiate(shotPrefab, aimingStartPosition, Utils.GetRorationOutwards(transform.position, aimingStartPosition));
            shot.transform.SetParent(gameObject.transform);
            shotsList.Add(shot);
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
            EndAttack();
            SetMoving();
        }
    }

    private void Shoot()
    {
        ShootRays(ShotPrefab, _shots);
        _didShoot = true;
    }

    private void DestroyShots(List<GameObject> shots)
    {
        foreach (var shot in shots)
        {
            Destroy(shot);
        }
    }

    private void EndAttack()
    {
        DestroyShots(_shots);
        _didShoot = false;
    }

    private void SetMoving()
    {
        SetMovingTime();
        _shootingLazerAsteriskEnemyState = MoveAimAttackEnemyState.MOVING;
    }

    private void SetMovingTime()
    {
        _movingTime = Time.time + _movingInterval;
    }

    private bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    private List<Vector2> GetShootingStartPositions(int numberOfPositions, float radius)
    {
        radius = 0.9f; // TODO: BUG - why is this 0.9 and not 0.5?
        var angle = 360 / numberOfPositions;
        var shootingStartPositions = new List<Vector2>();
        for (var i = 0; i < numberOfPositions; i++)
        {
            shootingStartPositions.Add(transform.position + Utils.AngleAndRadiusToPointOnCircle(angle * i, radius));
        }
        return shootingStartPositions;
    }

    private bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    private void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
        _shootingLazerAsteriskEnemyState = MoveAimAttackEnemyState.AIMING;
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