using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    private ShootingInRangeAim _shootingInRangeAim;
    [SerializeField]
    public GameObject _shotPrefab;
    
    private GameObject _projectiles;

    public void Start()
    {
        _projectiles = GameObject.Find(Constants.PROJECTILES_GAME_OBJECT_NAME);
    }

    public override void Shoot(Transform transform)
    {
        Vector3 shootingStartPosition = GetShootingStartPosition(transform);
        var shootRotation = Utils.GetRotationPlusAngle(transform.rotation, _shootingInRangeAim.GetShootingAngle());
        Instantiate(_shotPrefab, shootingStartPosition, shootRotation, _projectiles.transform);
    }

    private Vector3 GetShootingStartPosition(Transform transform)
    {
        Vector3 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _shootingInRangeAim.GetHalalitDirection().normalized * halfExtents.magnitude;
        return transform.position + offset;
    }
}