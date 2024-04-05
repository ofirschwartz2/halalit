using UnityEngine;

public class AsteroidEventArguments : System.EventArgs
{
    public Vector2 Position;
    public float Speed;
    public Vector2 Direction;
    public int Scale;

    public AsteroidEventArguments(Vector2 position, float speed, Vector2 direction, int scale)
    {
        Position = position;
        Speed = speed;
        Direction = direction;
        Scale = scale;
    }
}