using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class ShootingLaserAsteriskAttack : MoveAimAttackAttack
{
    [SerializeField]
    private GameObject _laserBeamPrefab;
    [SerializeField]
    private ShootingLaserAsteriskAim _shootingLazerAsteriskAim;
    [SerializeField]
    private float _endShootingBeforeFinishAttack;
    [SerializeField]
    private int _numberOfShots;

    private GameObject _projectiles;
    private List<ConsecutiveAttack> _laserBeams;
    private float _attackRadiusMultiplier = 1.1f; // So that the shots don't spawn inside the enemy // TODO (refactor): this should be configurable 

    void Start()
    {
        _laserBeams = new List<ConsecutiveAttack>();
        _projectiles = GameObject.Find(Constants.PROJECTILES_GAME_OBJECT_NAME);
    }

    public override void AttackingState(Transform transform)
    {
        if (!_didShoot)
        {
            Shoot(transform);
            _shootingLazerAsteriskAim.DestroyAimingShots();
            _didShoot = true;
        } 
        else
        {
            if (IsTimeToDestroyShots())
            {
                TryDestroyShots();
            }
            else
            {
                for (int i = 0; i < _laserBeams.Count; i++) 
                {
                    _laserBeams[i].UpdateConsecitiveAttack(GetShootingPoint(i.ToString()), GetShootingRotation(i.ToString()));
                }
            }
        }
    }

    public override void Shoot(Transform transform)
    {
        StartShootLaserBeams();
    }

    private void StartShootLaserBeams()
    {
        List<Vector2> attackingStartPositions = EnemyUtils.GetEvenPositionsAroundCircle(transform, _numberOfShots, GetComponent<CircleCollider2D>().radius * _attackRadiusMultiplier);
        for (int i = 0; i < attackingStartPositions.Count; i++)
        {
            StartShootOneLaserBeam(attackingStartPositions[i], i);
        }
    }

    private void StartShootOneLaserBeam(Vector2 startPosition, int laserBeamNumber) 
    {
        ConsecutiveAttack laserBeam = Instantiate(_laserBeamPrefab, startPosition, Utils.GetRorationOutwards(transform.position, startPosition), _projectiles.transform).GetComponent<ConsecutiveAttack>();

        laserBeam.StartConsecitiveAttack(GetShootingPoint(laserBeamNumber.ToString()), GetShootingRotation(laserBeamNumber.ToString()));
        laserBeam.gameObject.layer = LayerMask.NameToLayer("EnemyShots");
        _laserBeams.Add(laserBeam);
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
        foreach (ConsecutiveAttack laserBeam in _laserBeams) 
        {
            laserBeam.StopConsecitiveAttack();
        }
        _laserBeams.Clear();
    }

    private bool IsShotsAlive() 
    {
        return _laserBeams.Count > 0;
    }

    private bool IsTimeToDestroyShots() 
    {
        return Time.time > _attackingTime - _endShootingBeforeFinishAttack;
    }

    private void OnDestroy()
    {
        foreach (ConsecutiveAttack laserBeam in _laserBeams)
        {
            if (laserBeam != null)
            {
                Destroy(laserBeam.gameObject);
            }
        }
    }
}