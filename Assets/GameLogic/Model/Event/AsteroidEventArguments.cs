using UnityEngine;

public class AsteroidEventArguments : System.EventArgs
{
    public Vector2 AsteroidPosition;
    public Vector2 AsteroidVelocity;
    public float AsteroidScale;

    public AsteroidEventArguments(Vector2 destructionPosition, Vector2 destroyedAsteroidVelocity, float destroyedAsteroidScale)
    {
        AsteroidPosition = destructionPosition;
        AsteroidVelocity = destroyedAsteroidVelocity;
        AsteroidScale = destroyedAsteroidScale;
    }
}