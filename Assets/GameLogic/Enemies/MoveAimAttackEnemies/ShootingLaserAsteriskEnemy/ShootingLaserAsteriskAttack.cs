using Assets.Utils;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ShootingLaserAsteriskAttack : MoveAimAttackAttack
{
    [SerializeField]
    private GameObject ShotPrefab;
    [SerializeField]
    private ShootingLaserAsteriskAim _shootingLazerAsteriskAim;
    [SerializeField]
    private float _endShootingBeforeFinishAttack;
    [SerializeField]
    private int _numberOfShots;

    private List<GameObject> _shots;
    private float _attackRadiusMultiplier = 1.1f; // So that the shots don't spawn inside the enemy

    void Start()
    {
        _shots = new List<GameObject>();
    }

    public override void AttackingState(Transform transform)
    {
        if (!_didShoot)
        {
            Shoot(transform);
            _shootingLazerAsteriskAim.DestroyAimingShots();
            _didShoot = true;
        } else if (IsTimeToDestroyShots())
        {
            TryDestroyShots();
        }
        
    }

    public override void Shoot(Transform transform)
    {
        ShootRays();
    }

    private void ShootRays()
    {
        var attackingStartPositions = EnemyUtils.GetEvenPositionsAroundCircle(transform, _numberOfShots, GetComponent<CircleCollider2D>().radius * _attackRadiusMultiplier);
        foreach (var attackingStartPosition in attackingStartPositions)
        {
            ShootOneRay(attackingStartPosition);
        }
    }

    private void ShootOneRay(Vector2 startPosition) 
    {
        var shot = Instantiate(ShotPrefab, startPosition, Utils.GetRorationOutwards(transform.position, startPosition));
        shot.transform.SetParent(gameObject.transform);
        shot.layer = LayerMask.NameToLayer("EnemyShots");
        _shots.Add(shot);
    }

    private void TryDestroyShots() 
    {
        if (IsShotsAlive()) 
        {
            DestroyShots();
        }
    }

    private void DestroyShots() 
    {
        foreach (var shot in _shots) 
        {
            Destroy(shot);
        }
    }

    private bool IsShotsAlive() 
    {
        return _shots.Count > 0;
    }

    private bool IsTimeToDestroyShots() 
    {
        return Time.time > _attackingTime - _endShootingBeforeFinishAttack;
    }
}