using Assets.Utils;
using System;
using UnityEngine;

public class ShootingLaserRangeAim : MoveAimAttackAim
{
    [SerializeField]
    public GameObject AimShotPrefab;
    [SerializeField]
    private float _shootingRange;

    private GameObject _aimingShotFrom, _aimingShotTo;
    private float _aimingOffsetMultiplier = 1.1f;
    public override void Aim(Transform transform = null)
    {
        ShootAimingRays();
    }

    private void ShootAimingRays()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        _aimingShotFrom = ShootAimingShot(shootingStartPosition, -_shootingRange);
        _aimingShotTo = ShootAimingShot(shootingStartPosition, _shootingRange);
    }

    private GameObject ShootAimingShot(Vector3 shootingStartPosition, float angle)
    {
        Quaternion fromRotation = Utils.GetRotationPlusAngle(transform.rotation, angle);
        var aimShot = Instantiate(AimShotPrefab, shootingStartPosition, fromRotation);
        aimShot.transform.SetParent(gameObject.transform);
        aimShot.layer = LayerMask.NameToLayer("EnemyShots");
        return aimShot;
    }

    public Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _aimingOffsetMultiplier * halfExtents.magnitude * Utils.GetRotationAsVector2(transform.rotation);
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

    public void TurnOff()
    {
        _aimingShotFrom.GetComponent<Fader>().StartFadeOut();
        _aimingShotTo.GetComponent<Fader>().StartFadeOut();
    }

    public void DestroyAimingRays()
    {
        Destroy(_aimingShotFrom);
        Destroy(_aimingShotTo);
    }
}