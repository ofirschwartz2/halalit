using Assets.Utils;
using UnityEngine;

public class ShootingLaserRangeAttack : MoveAimAttackAttack
{
    [SerializeField]
    private GameObject _laserBeamPrefab;
    [SerializeField]
    private ShootingLaserRangeAim _shootingLaserRangeAim;
    [SerializeField]
    private float _shotRotationSpeed;

    private GameObject _projectiles;
    private bool _laserBeamInitiated;
    private ConsecutiveAttack _laserBeamInstance;
    
    void Start()
    {
        _laserBeamInitiated = false;
        _projectiles = GameObject.Find(Constants.PROJECTILES_GAME_OBJECT_NAME);
    }

    private void FixedUpdate()
    {
        if (_laserBeamInitiated)
        {
            RotateShot();
            TryDestroyShot();
        }
    }

    private void RotateShot()
    {
        _laserBeamInstance.UpdateConsecitiveAttack(GetShootingPoint(), Quaternion.Slerp(
            _laserBeamInstance.transform.rotation,
            _shootingLaserRangeAim.GetAimingShotTo().transform.rotation,
            _shotRotationSpeed * Time.deltaTime));
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
        return Quaternion.Angle(_laserBeamInstance.transform.rotation, _shootingLaserRangeAim.GetAimingShotTo().transform.rotation) < 2f; 
    }

    private void DestroyShot()
    {
        _laserBeamInitiated = false;

        _shootingLaserRangeAim.DestroyAimingRays();
        _laserBeamInstance.StopConsecitiveAttack();
    }

    public override void Shoot(Transform transform)
    {
        if (!_laserBeamInitiated)
        {
            _shootingLaserRangeAim.TurnOff();
            InitiateShot();
        }
    }

    private void InitiateShot() 
    {
        Vector3 shootingStartPosition = _shootingLaserRangeAim.GetShootingStartPosition();
        var shootRotation = _shootingLaserRangeAim.GetAimingShotFrom().transform.rotation;

        if (_laserBeamInstance == null)
        {
            _laserBeamInstance = Instantiate(_laserBeamPrefab, shootingStartPosition, shootRotation, _projectiles.transform).GetComponent<ConsecutiveAttack>();
        }
        
        _laserBeamInstance.StartConsecitiveAttack(GetShootingPoint(), transform.rotation);
        _laserBeamInitiated = true;
    }

    private void OnDestroy()
    {
        if (_laserBeamInstance != null)
        {
            Destroy(_laserBeamInstance.gameObject);
        }
    }
}