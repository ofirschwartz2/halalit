using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class AsteroidInternalInstantiator : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
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
    private float _asteroidScaleMultiplier;
    [SerializeField]
    private float _asteroidNewDirectionOffsetDegree;

    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        AsteroidEvent.AsteroidDestruction += InstantiateAsteroidsOnPosition;
    }

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }
    }

    private void InstantiateAsteroidsOnPosition(object initiator, AsteroidEventArguments arguments)
    {
        int newAsteroidsCount = GetNewAsteroidsCount(arguments.AsteroidScale);
        float newAsteroidsScale = GetNewAsteroidsScale(newAsteroidsCount, arguments.AsteroidScale);
        List<Vector2> newAsteroidPositions = GetNewAsteroidPositions(newAsteroidsCount, newAsteroidsScale, arguments.AsteroidPosition);
        List<Vector2> newAsteroidDirections = GetNewAsteroidDirections(newAsteroidsCount, arguments.AsteroidVelocity.normalized);

        for (int i = 0; i < newAsteroidPositions.Count; i++)
        {
            GameObject asteroid = Instantiate(_asteroidPrefab, newAsteroidPositions[i], Quaternion.identity);
            _asteroidInitiator.InitAsteroid(asteroid, newAsteroidDirections[i], newAsteroidsScale);
        }
    }

    private int GetNewAsteroidsCount(float originalAsteroidScale)
    {
        if (originalAsteroidScale >= _asteroidMaxScale)
        {
            return Random.Range((int) _asteroidMinInstantiationCount, (int) _asteroidMaxInstantiationCount + 1);
        }
        else if (originalAsteroidScale >= _asteroidMaxScale / 2)
        {
            return Random.Range((int) _asteroidMinInstantiationCount, ((int)_asteroidMaxInstantiationCount + 1) / 2);
        }

        return 0;
    }

    private float GetNewAsteroidsScale(int newAsteroidsCount, float originalAsteroidScale)
    {
        return (originalAsteroidScale / newAsteroidsCount) * _asteroidScaleMultiplier;
    }

    private List<Vector2> GetNewAsteroidPositions(int newAsteroidsCount, float newAsteroidsScale, Vector2 originalAsteroidPosition)
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
            return Utils.Vector2ToDegree(originalAsteroidDirection.x, originalAsteroidDirection.y);
        }

        float leftestOffsetDegree;
        float originalAsteroidDegree = Utils.Vector2ToDegree(originalAsteroidDirection.x, originalAsteroidDirection.y);

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
}
