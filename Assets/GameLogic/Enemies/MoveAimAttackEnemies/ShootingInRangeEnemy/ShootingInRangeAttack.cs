using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    private ShootingInRangeAim _shootingInRangeAim;
    [SerializeField]
    public GameObject ShotPrefab;

    public override void Shoot(Transform transform)
    {
        Vector3 shootingStartPosition = GetShootingStartPosition(transform);
        var shootRotation = Utils.GetRotationPlusAngle(transform.rotation, _shootingInRangeAim.GetShootingAngle());
        Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
    }

    private Vector3 GetShootingStartPosition(Transform transform)
    {
        Vector3 halfExtents = GetComponent<PolygonCollider2D>().bounds.extents;
        Vector3 offset = _shootingInRangeAim.GetHalalitDirection().normalized * halfExtents.magnitude;
        return transform.position + offset;
    }
}