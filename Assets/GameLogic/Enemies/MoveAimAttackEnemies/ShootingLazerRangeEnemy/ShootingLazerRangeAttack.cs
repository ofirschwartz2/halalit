using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ShootingLazerRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _shotRotationSpeed;
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private ShootingLazerRangeAim _shootingLazerRangeAim;

    private bool _shotInitiated;
    private GameObject _shot;
    private Quaternion shootUpRotationMultiplier = Quaternion.Euler(0f, 0f, 90f); // TODO: fix this BUG
    
    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
        _shotInitiated = false;
    }

    public override void AttackingState(Transform transform)
    {
        Shoot(transform);
    }

    public override void Shoot(Transform transform)
    {
        if (!_shotInitiated)
        {
            InitiateShot();
            _shotInitiated = true;
        }
        else 
        {
            RotateShot();
            TryDestroyShot();
        }
    }

    private void InitiateShot() 
    {
        Vector3 shootingStartPosition = GetShootingStartPosition();
        var shootRotation = _shootingLazerRangeAim.GetAimingShotFrom().transform.rotation;
        _shot = Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _shot.transform.SetParent(gameObject.transform);
    }

    private void RotateShot()
    {
        _shot.transform.rotation = Quaternion.Slerp(_shot.transform.rotation, _shootingLazerRangeAim.GetAimingShotTo().transform.rotation, _shotRotationSpeed);   
    }

    private void TryDestroyShot() 
    {
        if (DidShotGetToEndOfAimRange())
        {
            DestroyShot();
        }
    }

    private bool DidShotGetToEndOfAimRange()
    {
        return _shot.transform.rotation == _shootingLazerRangeAim.GetAimingShotTo().transform.rotation;
    }

    private void DestroyShot()
    {
        Debug.Log("DestroyShot");
        _shootingLazerRangeAim.DestroyAimingRays();
        Destroy(_shot);
        _shotInitiated = false;
    }

    private Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude;
        return transform.position + offset;
    }
}