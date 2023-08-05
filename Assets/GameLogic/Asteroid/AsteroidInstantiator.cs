using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidInstantiator : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private GameObject _asteroidPrefab;
    [SerializeField]
    private float _asteroidInstantiationLineLength;
    [SerializeField]
    private float _asteroidInstantiationInterval;
    [SerializeField]
    private float _asteroidInstantiationDistanceFromCenter;

    private float _time;
    private Vector2 _instantiationLineCenterPoint;
    private float _instantiationLineSlope;
    private Vector2 _asteroidsDirection;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _time = 0;
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
        _time += Time.deltaTime;

        if (_time >= _asteroidInstantiationInterval)
        {
            _time = 0;
            InstantiateAsteroidsFromStartLine();
        }
    }

    private void InstantiateAsteroidsFromStartLine()
    {
        List<Vector2> asteroidInstantiationPositions = GetAsteroidInstantiationPositions(1);

        foreach (Vector2 position in asteroidInstantiationPositions)
        {
            InstantiateDirectionalAsteroids(position);
        }
    }

    private void InstantiateDirectionalAsteroids(Vector2 position)
    {
        GameObject asteroid = Instantiate(_asteroidPrefab, position, Quaternion.identity);

        AsteroidMovement asteroidInstanceMovement = asteroid.GetComponent<AsteroidMovement>();
        asteroidInstanceMovement.SetDirection(_asteroidsDirection);

        asteroid.transform.SetParent(transform.parent);
    }

    private List<Vector2> GetAsteroidInstantiationPositions(int positionCount)
    {
        List<Vector2> asteroidInstantiationPositions = new();

        for (int i = 0; i < positionCount; i++)
        {
            float asteroidInstantiationPointX = GetRandomAsteroidInstantiationPointX();
            float asteroidInstantiationPointY = GetAsteroidInstantiationPointY(asteroidInstantiationPointX);

            asteroidInstantiationPositions.Add(new Vector2(asteroidInstantiationPointX, asteroidInstantiationPointY));
        }

        return asteroidInstantiationPositions;
    }

    private float GetRandomAsteroidInstantiationPointX()
    {
        float centerPointAndInstantiationPointDistance = Random.Range(0, _asteroidInstantiationDistanceFromCenter);

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
}