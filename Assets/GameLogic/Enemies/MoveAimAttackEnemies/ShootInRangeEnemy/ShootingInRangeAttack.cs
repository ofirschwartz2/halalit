using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingInRangeAttack : MonoBehaviour, IMoveAimAttackAttack
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private ShootingInRangeAim _shootingInRangeAim;
    [SerializeField] 
    private float _attackingInterval;
    [SerializeField]
    public GameObject ShotPrefab;

    private float _attackingTime;
    private bool _didShoot;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
    }

    public void AttackingState()
    {
        if (!_didShoot)
        {
            Shoot();
        }
    }

    public bool DidAttackingTimePass()
    {
        return Time.time > _attackingTime;
    }

    private void Shoot()
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        var shootRotation = Utils.GetRotation(transform.rotation, _shootingInRangeAim.GetShootingAngle());
        Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _didShoot = true;
    }

    private Vector3 GetShootingStartPosition()
    {
        Vector3 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = _shootingInRangeAim.GetHalalitDirection().normalized * halfExtents.magnitude;
        return transform.position + offset;
    }

    public void SetAttacking()
    {
        _attackingTime = Time.time + _attackingInterval;
        _didShoot = false;
    }
}