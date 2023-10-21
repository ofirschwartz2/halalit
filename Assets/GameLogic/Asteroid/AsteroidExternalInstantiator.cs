using Assets.Utils;
using System.Collections.Generic;
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
    private float _asteroidInstantiationLoad;
    [SerializeField]
    private float _asteroidInstantiationDistanceFromCenter;
    [SerializeField]
    private int _asteroidMaxScale;
    [SerializeField]
    private float _maxInstantiationsRetry;
    [SerializeField]
    private float _asteroidTakeOffTime;

    private List<KeyValuePair<float, RangeAttribute>> _forbiddenInstantiationZones;
    private Vector2 _instantiationLineCenterPoint;
    private Vector2 _asteroidsDirection;
    private float _instantiationLineSlope;
    private float _timeToInstantiation;

    void Start()
    {
        _timeToInstantiation = 0;
        _forbiddenInstantiationZones = new();
        _instantiationLineCenterPoint = GetRandomInstantiationLineCenterPoint();
        _instantiationLineSlope = GetAsteroidInstantiationLineSlope();
        _asteroidsDirection = GetAsteroidDirection();
    }

    private float GetAsteroidInstantiationLineSlope()
    {
        return -_instantiationLineCenterPoint.x / _instantiationLineCenterPoint.y;
    }

    private Vector2 GetRandomInstantiationLineCenterPoint()
    {
        return Random.insideUnitCircle.normalized * _asteroidInstantiationDistanceFromCenter;
    }

    private Vector2 GetAsteroidDirection()
    {
        return new Vector2(-_instantiationLineCenterPoint.x, -_instantiationLineCenterPoint.y).normalized;
    }

    void Update()
    {
        InstantiateAsteroidsFromStartLinePeriodicaly();
        RemoveOldForbiddenInstantiationZones();
    }

    #region Determine instantiation position
    private List<Vector2> GetAsteroidInstantiationPositions()
    {
        List<Vector2> asteroidInstantiationPositions = new();

        for (int i = 0; i < _asteroidInstantiationLoad; i++)
        {
            for (int j = 0; j < _maxInstantiationsRetry; j++)
            {
                float asteroidInstantiationPointX = GetRandomAsteroidInstantiationPointX();

                if (IsValidAsteroidInstantiationPointX(asteroidInstantiationPointX))
                {
                    float asteroidInstantiationPointY = GetAsteroidInstantiationPointY(asteroidInstantiationPointX);
                    asteroidInstantiationPositions.Add(new Vector2(asteroidInstantiationPointX, asteroidInstantiationPointY));
                    break;
                }
            }
        }

        return asteroidInstantiationPositions;
    }

    private bool IsValidAsteroidInstantiationPointX(float asteroidInstantiationPointX)
    {
        float asteroidLocalScale = _asteroidPrefab.transform.localScale.x;

        List<float> xPointsToCheck = new()
        {
            asteroidInstantiationPointX - asteroidLocalScale,
            asteroidInstantiationPointX - asteroidLocalScale * 2,
            asteroidInstantiationPointX,
            asteroidInstantiationPointX + asteroidLocalScale,
            asteroidInstantiationPointX + asteroidLocalScale * 2,
        };

        foreach (KeyValuePair<float, RangeAttribute> timedForbiddenInstantiationZone in _forbiddenInstantiationZones)
        {
            foreach (float xPointToCheck in xPointsToCheck) 
            {
                if (IsXPointInForbiddenRange(timedForbiddenInstantiationZone.Value, xPointToCheck))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool IsXPointInForbiddenRange(RangeAttribute forbiddenInstantiationZone, float xPoint)
    {
        return forbiddenInstantiationZone.min <= xPoint && forbiddenInstantiationZone.max >= xPoint;
    }

    private float GetRandomAsteroidInstantiationPointX()
    {
        float centerPointAndInstantiationPointDistance = Random.Range(0, _asteroidInstantiationDistanceFromCenter);

        float a = _instantiationLineCenterPoint.x;
        float b = centerPointAndInstantiationPointDistance / Mathf.Sqrt(Mathf.Pow(_instantiationLineSlope, 2) + 1);

        if (Utils.IsTrueOneOf(2))
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

    private void RemoveOldForbiddenInstantiationZones()
    {
        for (int i = 0; i < _forbiddenInstantiationZones.Count; i++)
        {
            _forbiddenInstantiationZones[i].Key -= Time.deltaTime;

            if (_forbiddenInstantiationZones[i].Key <= 0)
            {
                _forbiddenInstantiationZones.RemoveAt(i);
            }
        }
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
        AddToForbiddenInstantiationZone(asteroid);
    }

    private void AddToForbiddenInstantiationZone(GameObject asteroid)
    {
        float asteroidInstantiationCenterX = asteroid.transform.position.x;
        float halfAsteroidXScale = asteroid.transform.localScale.x / 2;

        RangeAttribute newForbiddenInstantiationZone = new(asteroidInstantiationCenterX - halfAsteroidXScale, asteroidInstantiationCenterX + halfAsteroidXScale);

        _forbiddenInstantiationZones.Add(new KeyValuePair<float, RangeAttribute>(_asteroidTakeOffTime, newForbiddenInstantiationZone));
    }

    private float GetRandomAsteroidScale()
    {
        return Random.Range(1, _asteroidMaxScale + 1);
    }
    #endregion
}