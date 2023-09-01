using Assets.Utils;
using UnityEngine;

public class ShootingLazerRangeAim : MoveAimAttackAim
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _shootingRange;
    [SerializeField]
    public GameObject AimShotPrefab;

    private GameObject _aimingShotFrom, _aimingShotTo;
    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
    }

    public override void Aim(Transform transform = null)
    {
        ShootAimingRays();
    }

    private void ShootAimingRays()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        _aimingShotFrom = ShootAimingShot(shootingStartPosition, -_shootingRange);
        _aimingShotTo = ShootAimingShot(shootingStartPosition, _shootingRange);
        //ShootAimingShotTo(shootingStartPosition);
    }

    private GameObject ShootAimingShot(Vector3 shootingStartPosition, float angle)
    {
        Quaternion fromRotation = Utils.GetRotation(transform.rotation, angle);
        var aimShot = Instantiate(AimShotPrefab, shootingStartPosition, fromRotation);
        aimShot.transform.SetParent(gameObject.transform);
        aimShot.layer = LayerMask.NameToLayer("EnemyShots");
        return aimShot;
    }

    private void ShootAimingShotFrom(Vector3 shootingStartPosition)
    {
        Quaternion fromRotation = Utils.GetRotation(transform.rotation, -_shootingRange);
        _aimingShotFrom = Instantiate(AimShotPrefab, shootingStartPosition, fromRotation);
        _aimingShotFrom.transform.SetParent(gameObject.transform);
        _aimingShotFrom.layer = LayerMask.NameToLayer("EnemyShots");
    }

    private void ShootAimingShotTo(Vector3 shootingStartPosition)
    {
        Quaternion toRotation = Utils.GetRotation(transform.rotation, _shootingRange);
        _aimingShotTo = Instantiate(AimShotPrefab, shootingStartPosition, toRotation);
        _aimingShotTo.transform.SetParent(gameObject.transform);
        _aimingShotTo.layer = LayerMask.NameToLayer("EnemyShots");
    }

    private Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude;
        return transform.position + offset;
    }

    public GameObject GetAimingShotFrom() 
    {
        return _aimingShotFrom;
    }

    public GameObject GetAimingShotTo()
    {
        return _aimingShotTo;
    }

    public void DestroyAimingRays()
    {
        Destroy(_aimingShotFrom);
        Destroy(_aimingShotTo);
    }

}