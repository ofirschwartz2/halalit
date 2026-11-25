using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAim : MoveAimAttackAim
{
    [SerializeField]
    private float _shootingRange;

    private Vector2 _halalitDirection;
    private float _shootingAngle;
    private EnemySharedBehavior _enemySharedBehavior;

    void Awake()
    {
        _enemySharedBehavior = GetComponent<EnemySharedBehavior>();
    }
    
    public override void Aim(Transform transform)
    {
        _halalitDirection = EnemyTarget.GetDirection(transform.position);
        _shootingAngle = Utils.GetRandomAngleAround(_enemySharedBehavior._seedfulRandomGenerator, _shootingRange);
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