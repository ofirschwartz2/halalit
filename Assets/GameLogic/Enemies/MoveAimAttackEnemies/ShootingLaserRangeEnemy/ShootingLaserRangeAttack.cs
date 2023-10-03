using UnityEngine;

public class ShootingLaserRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    public GameObject ShotPrefab;
    [SerializeField]
    private ShootingLazerRangeAim _shootingLazerRangeAim;
    [SerializeField]
    private float _shotRotationSpeed; // If too small - BUG.

    private bool _shotInitiated;
    private GameObject _shot;
    
    void Start()
    {
        _shotInitiated = false;
    }

    private void FixedUpdate()
    {
        if (_shotInitiated)
        {
            RotateShot();
            TryDestroyShot();
        }
    }

    public override void Shoot(Transform transform)
    {
        if (!_shotInitiated)
        {
            Debug.Log("Shot initiated");
            InitiateShot();
            _shotInitiated = true;
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
        return Quaternion.Angle(_shot.transform.rotation, _shootingLazerRangeAim.GetAimingShotTo().transform.rotation) < 2f ;
    }

    private void DestroyShot()
    {
        _shotInitiated = false;

        _shootingLazerRangeAim.DestroyAimingRays();
        Destroy(_shot);
    }

}