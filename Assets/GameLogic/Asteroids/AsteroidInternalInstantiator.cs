using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidInternalInstantiator : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private GameObject _asteroidPrefab;
    [SerializeField]
    private AsteroidInitiator _asteroidInitiator;
    [SerializeField]
    private float _asteroidMaxScale;
    [SerializeField]
    private float _asteroidMaxInstantiationCount;
    [SerializeField]
    private float _asteroidMinInstantiationCount;
    [SerializeField]
    private float _asteroidNewDirectionOffsetDegree;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        AsteroidEvent.AsteroidInternalInstantiation += InstantiateAsteroidsOnPosition;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        AsteroidEvent.AsteroidInternalInstantiation -= InstantiateAsteroidsOnPosition;
    }
    #endregion

    #region Internal instantiation
    private void InstantiateAsteroidsOnPosition(object initiator, AsteroidEventArguments arguments)
    {
        if (ShouldInstantiateAsteroidsInternaly(arguments.Scale))
        {
            int newAsteroidsCount = GetNewAsteroidsCount(arguments.Scale);
            int newAsteroidsScale = GetNewAsteroidsScale(arguments.Scale);
            List<Vector2> newAsteroidPositions = GetNewAsteroidPositions(newAsteroidsCount, arguments.Position);
            List<Vector2> newAsteroidDirections = GetNewAsteroidDirections(newAsteroidsCount, arguments.Direction);
            string newSiblingAsteroidsId = Guid.NewGuid().ToString();

            for (int i = 0; i < newAsteroidPositions.Count; i++)
            {
                GameObject asteroid = Instantiate(_asteroidPrefab, newAsteroidPositions[i], Quaternion.identity);
                _asteroidInitiator.InitAsteroid(asteroid, newAsteroidDirections[i], newAsteroidsScale, newSiblingAsteroidsId);
            }
        }
    }

    private int GetNewAsteroidsCount(int originalAsteroidScale)
    {
        return RandomGenerator.Range((int)_asteroidMinInstantiationCount, (int)_asteroidMaxInstantiationCount + 1, true);
    }

    private int GetNewAsteroidsScale(int originalAsteroidScale)
    {
        return originalAsteroidScale / 2;
    }

    private List<Vector2> GetNewAsteroidPositions(int newAsteroidsCount, Vector2 originalAsteroidPosition)
    {
        List<Vector2> newAsteroidPositions = new();

        for (int i = 0; i < newAsteroidsCount; i++)
        {
            newAsteroidPositions.Add(originalAsteroidPosition);
        }

        return newAsteroidPositions;
    }

    private List<Vector2> GetNewAsteroidDirections(int newAsteroidsCount, Vector2 originalAsteroidDirection)
    {
        List<Vector2> newAsteroidDirections = new();
        float leftestNewDirection = GetLeftestNewDirection(newAsteroidsCount, originalAsteroidDirection);

        for (int i = 0; i < newAsteroidsCount; i++)
        {
            float newAsteroidDirection = Utils.GetNormalizedAngleBy360(leftestNewDirection + _asteroidNewDirectionOffsetDegree * i);
            newAsteroidDirections.Add(Utils.DegreeToVector2(newAsteroidDirection)); 
        }

        return newAsteroidDirections;
    }

    private float GetLeftestNewDirection(int newAsteroidsCount, Vector2 originalAsteroidDirection)
    {
        if (newAsteroidsCount == 1)
        {
            return Utils.Vector2ToDegrees(originalAsteroidDirection.x, originalAsteroidDirection.y);
        }

        float leftestOffsetDegree;
        float originalAsteroidDegree = Utils.Vector2ToDegrees(originalAsteroidDirection.x, originalAsteroidDirection.y);

        if (newAsteroidsCount % 2 == 0)
        {
            leftestOffsetDegree = _asteroidNewDirectionOffsetDegree * ((newAsteroidsCount / 2) - 0.5f);
        }
        else
        {
            leftestOffsetDegree = _asteroidNewDirectionOffsetDegree * (newAsteroidsCount - 2) / 2;
        }

        return originalAsteroidDegree - leftestOffsetDegree;
    }
    #endregion

    #region Predicates
    private bool ShouldInstantiateAsteroidsInternaly(int originalAsteroidScale)
    {
        return originalAsteroidScale > 1 && RandomGenerator.IsTrueIn50Precent(false);
    }
    #endregion
}
