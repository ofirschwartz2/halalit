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

    private bool _shotInitiated;
    private GameObject _aimingShotFrom, _aimingShotTo, _shot;
    private Quaternion shootUpRotationMultiplier = Quaternion.Euler(0f, 0f, 90f); // TODO: fix this BUG
    
    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
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
        var shootRotation = _aimingShotFrom.transform.rotation;
        _shot = Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _shot.transform.SetParent(gameObject.transform);
    }

    private void RotateShot()
    {
        _shot.transform.rotation = Quaternion.Slerp(_shot.transform.rotation, _aimingShotTo.transform.rotation, _shotRotationSpeed);   
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
        return _shot.transform.rotation == _aimingShotTo.transform.rotation;
    }

    private void DestroyShot()
    {
        Destroy(_shot);
        Destroy(_aimingShotFrom);
        Destroy(_aimingShotTo);
    }

    private Vector2 GetShootingStartPosition()
    {
        Vector2 halfExtents = GetComponent<CapsuleCollider2D>().bounds.extents;
        Vector3 offset = Utils.GetRotationAsVector2(transform.rotation) * halfExtents.magnitude;
        return transform.position + offset;
    }
}