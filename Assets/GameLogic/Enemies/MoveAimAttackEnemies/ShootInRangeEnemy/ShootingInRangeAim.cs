using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class ShootingInRangeAim : MonoBehaviour, IMoveAimAttackAim
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _aimingInterval;
    [SerializeField]
    private float _shootingRange;

    private Vector2 _halalitDirection;
    private float _aimingTime, _shootingAngle;
    private bool _didAim;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
     }

    public void AimingState()
    {
        if (!_didAim) 
        {
            Aim();
        }
    }

    private void Aim()
    {
        _halalitDirection = Utils.GetHalalitDirection(transform.position);
        _shootingAngle = Utils.GetRandomAngleAround(_shootingRange);
        _didAim = true;
    }

    public bool DidAimingTimePass()
    {
        return Time.time > _aimingTime;
    }

    public void SetAiming()
    {
        _aimingTime = Time.time + _aimingInterval;
        _didAim = false;
    }

    public float GetShootingAngle()
    { 
        return _shootingAngle; 
    }

    public Vector2 GetHalalitDirection()
    {
        return _halalitDirection;
    }
}