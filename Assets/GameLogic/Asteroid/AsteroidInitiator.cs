﻿using Assets.Enums;
using Assets.Utils;
using UnityEngine;

class AsteroidInitiator : MonoBehaviour
{
    public void InitAsteroid(GameObject asteroid, Vector2 direction, float scale)
    {
        AsteroidMovement asteroidInstanceMovement = asteroid.GetComponent<AsteroidMovement>();
        asteroidInstanceMovement.SetDirection(direction);

        asteroid.transform.localScale = new Vector3(scale, scale, 0);

        asteroid.tag = Tag.ASTEROID.GetDescription();

        asteroid.transform.SetParent(transform.parent);
    }
}