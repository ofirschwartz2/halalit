using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class AsteroidExternalInstantiator : MonoBehaviour
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

    private Vector2 _instantiationLineCenterPoint;
    private Vector2 _asteroidsDirection;
    private float _instantiationLineSlope;
    private float _timeToInstantiation;

    void Start()
    {
        _timeToInstantiation = 0;
        _instantiationLineCenterPoint = GetRandomInstantiationLineCenterPoint();
        _instantiationLineSlope = GetAsteroidInstantiationLineSlope();
        _asteroidsDirection = GetAsteroidDirection();
    }

    void Update()
    {
        InstantiateAsteroidsFromStartLinePeriodicaly();
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
        float centerPointAndInstantiationPointDistance = RandomGenerator.Range(0, _asteroidInstantiationDistanceFromCenter, true);

        float a = _instantiationLineCenterPoint.x;
        float b = centerPointAndInstantiationPointDistance / Mathf.Sqrt(Mathf.Pow(_instantiationLineSlope, 2) + 1);

        if (Utils.IsTrueIn50Precent())
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
        _asteroidInitiator.InitAsteroid(asteroid, _asteroidsDirection, GetRandomAsteroidScale());
    }

    private float GetRandomAsteroidScale()
    {
        return RandomGenerator.Range(1, (float)(_asteroidMaxScale + 1), true);
    }
    #endregion

    #region Accessors
    private float GetAsteroidInstantiationLineSlope()
    {
        return -_instantiationLineCenterPoint.x / _instantiationLineCenterPoint.y;
    }

    private Vector2 GetRandomInstantiationLineCenterPoint()
    {
        return RandomGenerator.GetInsideUnitCircle(true).normalized * _asteroidInstantiationDistanceFromCenter;
    }

    private Vector2 GetAsteroidDirection()
    {
        return new Vector2(-_instantiationLineCenterPoint.x, -_instantiationLineCenterPoint.y).normalized;
    }
    #endregion
}