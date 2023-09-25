using UnityEngine;

class FixedCollisionHarmer : CollisionHarmer
{
    [SerializeField]
    private int _collisionHarm;

    public override int GetCollisionHarm()
    {
        return _collisionHarm;
    }
}