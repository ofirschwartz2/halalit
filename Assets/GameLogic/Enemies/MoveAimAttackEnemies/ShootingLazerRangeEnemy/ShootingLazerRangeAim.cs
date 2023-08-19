using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class ShootingLazerRangeAim : MoveAimAttackAim
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

    public override void Aim(Transform transform = null)
    {
        ShootAimingRays();
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
        _aimingShotFrom = Instantiate(AimShotPrefab, shootingStartPosition, fromRotation * shootUpRotationMultiplier); // shootUpRotationMultiplier BUG
        _aimingShotFrom.transform.SetParent(gameObject.transform);
    }

    private void ShootAimingShotTo(Vector3 shootingStartPosition)
    {
        Quaternion toRotation = Utils.GetRotation(transform.rotation, _shootingRange);
        _aimingShotTo = Instantiate(AimShotPrefab, shootingStartPosition, toRotation * shootUpRotationMultiplier); // shootUpRotationMultiplier BUG
        _aimingShotTo.transform.SetParent(gameObject.transform);
    }

    private Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude;
        return transform.position + offset;
    }

    private bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    private void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
        _shootingLazerRangeEnemyState = MoveAimAttackEnemyState.AIMING;
    }
}