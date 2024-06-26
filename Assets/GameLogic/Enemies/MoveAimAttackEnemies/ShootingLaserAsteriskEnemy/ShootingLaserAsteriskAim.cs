using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class ShootingLaserAsteriskAim : MoveAimAttackAim
{
    [SerializeField]
    private GameObject AimShotPrefab;
    [SerializeField]
    private int _numberOfShots;

    private List<GameObject> _aimingShots;

    void Start()
    {
        _aimingShots = new List<GameObject>();
    }
    
    public override void Aim(Transform transform)
    {
        ShootRays(AimShotPrefab, _aimingShots);
    }

    private void ShootRays(GameObject shotPrefab, List<GameObject> shotsList)
    {
        var aimingStartPositions = EnemyUtils.GetEvenPositionsAroundCircle(transform, _numberOfShots, GetComponent<CircleCollider2D>().radius);
        foreach (var aimingStartPosition in aimingStartPositions)
        {
            ShootOneRay(aimingStartPosition);
        }
    }

    private void ShootOneRay(Vector2 startPosition)
    {
        var shootingRotation = Utils.GetRorationOutwards(transform.position, startPosition);
        var shot = Instantiate(AimShotPrefab, startPosition, shootingRotation);
        shot.transform.SetParent(gameObject.transform);
        shot.layer = LayerMask.NameToLayer("EnemyShots");
        _aimingShots.Add(shot);
    }

    public void DestroyAimingShots() 
    {
        foreach (var aimingShot in _aimingShots) 
        {
            Destroy(aimingShot);
        }
    }
}