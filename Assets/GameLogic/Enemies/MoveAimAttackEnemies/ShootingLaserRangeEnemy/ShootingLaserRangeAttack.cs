using UnityEngine;

public class ShootingLaserRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private ShootingLazerRangeAim _shootingLazerRangeAim;
    [SerializeField]
    private float _shotRotationSpeed; // If too small - BUG.

    private bool _shotInitiated, _shotDestroyed;
    private GameObject _shot;
    
    void Start()
    {
        _shotInitiated = false;
        _shotDestroyed = false;
    }

    public override void AttackingState(Transform transform)
    {
        if (!_shotDestroyed) 
        {
            Shoot(transform);
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
        Vector3 shootingStartPosition = _shootingLazerRangeAim.GetShootingStartPosition();
        var shootRotation = _shootingLazerRangeAim.GetAimingShotFrom().transform.rotation;
        _shot = Instantiate(ShotPrefab, shootingStartPosition, shootRotation);
        _shot.transform.SetParent(gameObject.transform);
    }

    private void RotateShot()
    {
        _shot.transform.rotation = Quaternion.Slerp(_shot.transform.rotation, _shootingLazerRangeAim.GetAimingShotTo().transform.rotation, _shotRotationSpeed * Time.deltaTime);  // TODO: check if bug? Time.deltaTime
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
        _shootingLazerRangeAim.DestroyAimingRays();
        Destroy(_shot);
        _shotInitiated = false;
        _shotDestroyed = true;
    }
}