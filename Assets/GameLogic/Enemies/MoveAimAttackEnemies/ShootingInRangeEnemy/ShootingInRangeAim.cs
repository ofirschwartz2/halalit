using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAim : MoveAimAttackAim
{
    [SerializeField]
    private float _shootingRange;

    private Vector2 _halalitDirection;
    private float _shootingAngle;
    
    public override void Aim(Transform transform)
    {
        _halalitDirection = Utils.GetHalalitDirection(transform.position);
        _shootingAngle = Utils.GetRandomAngleAround(_shootingRange);
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