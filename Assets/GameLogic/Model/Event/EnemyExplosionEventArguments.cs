using Assets.Enums;
using UnityEngine;

public class EnemyExplosionEventArguments : System.EventArgs
{
    public Vector2 Position;

    public EnemyExplosionEventArguments(Vector2 position)
    {
        Position = position;
    }
}