using Assets.Utils;
using UnityEngine;

public class AsteroidInitiator : MonoBehaviour
{
    public void InitAsteroid(GameObject asteroid, Vector2 direction, float scale, SeedfulRandomGenerator seedfulRandomGenerator, string newSiblingAsteroidsId = null)
    {
        AsteroidSharedBehavior asteroidSharedBehavior = asteroid.GetComponent<AsteroidSharedBehavior>();
        asteroidSharedBehavior.SetInitialSeedfulRandomGenerator(seedfulRandomGenerator.GetNumber());

        AsteroidMovement asteroidInstanceMovement = asteroid.GetComponent<AsteroidMovement>();
        asteroidInstanceMovement.SetDirection(direction);
        asteroidInstanceMovement.SetSiblingId(newSiblingAsteroidsId);

        asteroid.transform.localScale = new Vector3(scale, scale, 0);
        asteroid.transform.SetParent(transform.parent);
        
    }
}
