using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private ShootingInRangeAim _shootingInRangeAim;
    [SerializeField]
    public GameObject ShotPrefab;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
    }

    public override void Shoot(Transform transform)
    {
        Vector3 shootingStartPosition = GetShootingStartPosition(transform);
        var shootRotation = Utils.GetRotation(transform.rotation, _shootingInRangeAim.GetShootingAngle());
        Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
    }

    private Vector3 GetShootingStartPosition(Transform transform)
    {
        Vector3 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _shootingInRangeAim.GetHalalitDirection().normalized * halfExtents.magnitude;
        return transform.position + offset;
    }
}