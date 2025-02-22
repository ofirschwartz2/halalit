using Assets.Utils;
using System.Collections;
using UnityEngine;

public class SpawnHoleInstantiator : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private float[] _instantiationRate;
    [SerializeField]
    private GameObject _spawnHolePrefab;
    [SerializeField]
    private GameObject _internalWorld;
    [SerializeField]
    private float _instantiationDistanceFree;
    [SerializeField]
    private int _findPositionRetries;

    private int _instantiationRatesIndex;
    private float _nextInstantiationTime;
    private float _minX, _maxX, _minY, _maxY;
    private Vector3 _instantiationPosition;

    void Start()
    {
        _instantiationRatesIndex = 0;
        _nextInstantiationTime = GetNextInstantiationTime();
        SetInstantiationBounds();

        StartCoroutine(UpdateCoroutine());
    }
    private IEnumerator UpdateCoroutine()
    {
        while (true) 
        {
            if (Time.time >= _nextInstantiationTime)
            {
                _nextInstantiationTime = GetNextInstantiationTime();
                InstantiateSpawnHole();
            }

            yield return new WaitForSeconds(1f);
        }
    }

    private void InstantiateSpawnHole()
    {
        if (FindObjectOfType<EnemyBank>().NoMoreEnemies())
        {
            Destroy(gameObject);
        }
        else
        {
            TryInstantiatingSpawnHole();
        }
    }

    private void TryInstantiatingSpawnHole()
    {
        if (TryGetRandomInstantiationPosition())
        {
            Instantiate(_spawnHolePrefab, _instantiationPosition, Quaternion.identity);
        }
        else
        {
            _nextInstantiationTime = _nextInstantiationTime + 1f;
        }
    }

    private float GetNextInstantiationTime()
    {

        var nextInstantiationTime = Time.time + _instantiationRate[_instantiationRatesIndex];

        if (_instantiationRatesIndex != _instantiationRate.Length - 1)
        {
            _instantiationRatesIndex ++;
        }

        return nextInstantiationTime;
    }

    private bool TryGetRandomInstantiationPosition()
    {
        for (int i = 0; i < _findPositionRetries; i++)
        {
            var potentialPosition = new Vector2(_seedfulRandomGenerator.Range(_minX, _maxX), _seedfulRandomGenerator.Range(_minY, _maxY));
            Collider2D[] colliders = Physics2D.OverlapCircleAll(potentialPosition, _instantiationDistanceFree);
            if (colliders.Length == 1)
            {
                _instantiationPosition = potentialPosition;
                return true;
            }
        }

        return false;
    }

    private void SetInstantiationBounds()
    {
        var internalWorldBounds = _internalWorld.GetComponent<Collider2D>().bounds;

        _minX = internalWorldBounds.min.x + _instantiationDistanceFree;
        _maxX = internalWorldBounds.max.x - _instantiationDistanceFree;
        _minY = internalWorldBounds.min.y + _instantiationDistanceFree;
        _maxY = internalWorldBounds.max.y - _instantiationDistanceFree;
    }
}