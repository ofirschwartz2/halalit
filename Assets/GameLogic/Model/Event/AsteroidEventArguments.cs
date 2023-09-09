using UnityEngine;

public class AsteroidEventArguments : System.EventArgs
{
    public Vector2 Position;
    public Vector2 Velocity;
    public float Scale;

    public AsteroidEventArguments(Vector2 position, Vector2 velocity, float scale)
    {
        Position = position;
        Velocity = velocity;
        Scale = scale;
    }
}