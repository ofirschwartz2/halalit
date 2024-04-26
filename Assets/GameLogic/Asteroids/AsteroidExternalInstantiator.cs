using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidExternalInstantiator : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private GameObject _asteroidPrefab;
    [SerializeField]
    private AsteroidInitiator _asteroidInitiator;
    [SerializeField]
    private float _asteroidInstantiationLineLength;
    [SerializeField]
    private float _asteroidInstantiationTimeInterval;
    [SerializeField]
    private float _asteroidInstantiationDistanceFromCenter;
    [SerializeField]
    private int _asteroidMaxScale;
    [SerializeField]
    private float _maxInstantiationsRetry;
    [SerializeField]
    private int _maxInstantiations;

    private Vector2 _instantiationLineCenterPoint;
    private Vector2 _asteroidsDirection;
    private float _instantiationLineSlope;
    private float _timeToInstantiation;
    private int _instantiatedAsteroidsCount;

    void Start()
    {
        _timeToInstantiation = 0;
        _instantiatedAsteroidsCount = 0;
        _instantiationLineCenterPoint = GetRandomInstantiationLineCenterPoint();
        _instantiationLineSlope = GetAsteroidInstantiationLineSlope();
        _asteroidsDirection = GetAsteroidDirection();
    }

    void Update()
    {
        if (_instantiatedAsteroidsCount < _maxInstantiations)
        {
            InstantiateAsteroidsFromStartLinePeriodicaly();
        }
        else 
        {
            Destroy(this);
        }
    }

    #region Determine instantiation position
    private List<Vector2> GetAsteroidInstantiationPositions()
    {
        List<Vector2> asteroidInstantiationPositions = new();

        for (int i = 0; i < _maxInstantiationsRetry; i++)
        {
            float asteroidInstantiationPointX = GetRandomAsteroidInstantiationPointX();
            float asteroidInstantiationPointY = GetAsteroidInstantiationPointY(asteroidInstantiationPointX);
            Vector2 asteroidInstantiationPoint = new(asteroidInstantiationPointX, asteroidInstantiationPointY);

            if (IsValidAsteroidInstantiationPointX(asteroidInstantiationPoint))
            {
                asteroidInstantiationPositions.Add(asteroidInstantiationPoint);
                break;
            }
        }

        return asteroidInstantiationPositions;
    }

    private bool IsValidAsteroidInstantiationPointX(Vector2 asteroidInstantiationPoint)
    {
        return Physics2D.OverlapCircleAll(asteroidInstantiationPoint, _asteroidMaxScale)
            .Where(collider => collider.gameObject.CompareTag(Tag.ASTEROID.GetDescription())).ToArray().Length == 0;
    }

    private float GetRandomAsteroidInstantiationPointX()
    {
        float centerPointAndInstantiationPointDistance = _seedfulRandomGenerator.Range(0, _asteroidInstantiationDistanceFromCenter);

        float a = _instantiationLineCenterPoint.x;
        float b = centerPointAndInstantiationPointDistance / Mathf.Sqrt(Mathf.Pow(_instantiationLineSlope, 2) + 1);

        if (_seedfulRandomGenerator.IsTrueIn50Precent())
        {
            return a + b;
        }
        else
        {
            return a - b;
        }
    }

    private float GetAsteroidInstantiationPointY(float asteroidInstantiationPointX)
    {
        float a = _instantiationLineSlope * asteroidInstantiationPointX;
        float b = Mathf.Pow(_instantiationLineCenterPoint.x, 2) / _instantiationLineCenterPoint.y;
        float c = _instantiationLineCenterPoint.y;

        return a + b + c;
    }
    #endregion

    #region Instantiation process
    private void InstantiateAsteroidsFromStartLinePeriodicaly()
    {
        _timeToInstantiation += Time.deltaTime;

        if (_timeToInstantiation >= _asteroidInstantiationTimeInterval)
        {
            _timeToInstantiation = 0;
            InstantiateAsteroidsFromStartLine();
        }
    }

    private void InstantiateAsteroidsFromStartLine()
    {
        List<Vector2> asteroidInstantiationPositions = GetAsteroidInstantiationPositions();

        foreach (Vector2 position in asteroidInstantiationPositions)
        {
            InstantiateDirectionalAsteroids(position);
        }
    }

    private void InstantiateDirectionalAsteroids(Vector2 position)
    {
        GameObject asteroid = Instantiate(_asteroidPrefab, position, Quaternion.identity);
        _asteroidInitiator.InitAsteroid(asteroid, _asteroidsDirection, GetRandomAsteroidScale(), _seedfulRandomGenerator);
        _instantiatedAsteroidsCount++;
    }

    private float GetRandomAsteroidScale()
    {
        return _seedfulRandomGenerator.Range(1, (float)(_asteroidMaxScale + 1));
    }
    #endregion

    #region Accessors
    private float GetAsteroidInstantiationLineSlope()
    {
        return -_instantiationLineCenterPoint.x / _instantiationLineCenterPoint.y;
    }

    private Vector2 GetRandomInstantiationLineCenterPoint()
    {
        return _seedfulRandomGenerator.GetInsideUnitCircle().normalized * _asteroidInstantiationDistanceFromCenter;
    }

    private Vector2 GetAsteroidDirection()
    {
        return new Vector2(-_instantiationLineCenterPoint.x, -_instantiationLineCenterPoint.y).normalized;
    }
    #endregion
}