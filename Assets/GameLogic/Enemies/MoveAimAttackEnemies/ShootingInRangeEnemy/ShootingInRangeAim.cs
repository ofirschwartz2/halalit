using Assets.Utils;
using UnityEngine;

public class ShootingInRangeAim : MoveAimAttackAim
{
    [SerializeField]
    private bool _useConfigFile;

    [SerializeField]
    private float _shootingRange;

    private Vector2 _halalitDirection;
    private float _shootingAngle;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
    }
    
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