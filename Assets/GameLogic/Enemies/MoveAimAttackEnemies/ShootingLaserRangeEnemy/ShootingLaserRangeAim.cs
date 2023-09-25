using Assets.Utils;
using UnityEngine;

public class ShootingLazerRangeAim : MoveAimAttackAim
{
    [SerializeField]
    public GameObject AimShotPrefab;
    [SerializeField]
    private float _shootingRange;

    private GameObject _aimingShotFrom, _aimingShotTo;

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
        Quaternion fromRotation = Utils.GetRotation(transform.rotation, angle);
        var aimShot = Instantiate(AimShotPrefab, shootingStartPosition, fromRotation);
        aimShot.transform.SetParent(gameObject.transform);
        aimShot.layer = LayerMask.NameToLayer("EnemyShots");
        return aimShot;
    }

    public Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude * 0.85f;
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