using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class SpawnHoleInstantiator : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float[] _instantiationRate;
    [SerializeField]
    private List<KeyValuePair<GameObject, int>> _enemyPrefabsBank;
    [SerializeField]
    private GameObject _spawnHolePrefab;
    [SerializeField]
    private GameObject _internalWorld;
    [SerializeField]
    private float _instantiationDistanceFree;
    [SerializeField]
    private int _findPositionRetries;
    [SerializeField]
    private int _minSpawnEnemyCount;
    [SerializeField]
    private int _maxSpawnEnemyCount;

    private int _instantiationRatesIndex;
    private float _nextInstantiationTime;
    private float _minX, _maxX, _minY, _maxY;
    private Vector3 _instantiationPosition;
    private List<GameObject> _enemyPrefabList;
    private List<int> _spawnHoleSizesList;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _instantiationRatesIndex = 0;
        _nextInstantiationTime = GetNextInstantiationTime();
        SetInstantiationBounds();
        SetEnemiesList();
        SetSpawnHoleSizesList();
    }

    void Update()
    {
        if (Time.time >= _nextInstantiationTime)
        {
            _nextInstantiationTime = GetNextInstantiationTime();
            InstantiateSpawnHole();
        }
    }

    private void SetEnemiesList()
    {
        _enemyPrefabList = new List<GameObject>();
        foreach (var enemyPrefab in _enemyPrefabsBank)
        {
            for (int i = 0; i < enemyPrefab.Value; i++)
            {
                _enemyPrefabList.Add(enemyPrefab.Key);
            }
        }

        Utils.ShuffleList(_enemyPrefabList);
    }

    private void SetSpawnHoleSizesList()
    {
        var numberofEnemies = _enemyPrefabList.Count;

        while (numberofEnemies > 0)
        {
            var spawnHoleSize = Random.Range(_minSpawnEnemyCount, _maxSpawnEnemyCount);
            if (spawnHoleSize > numberofEnemies)
            {
                spawnHoleSize = numberofEnemies;
            }
            _spawnHoleSizesList.Add(spawnHoleSize);
            numberofEnemies -= spawnHoleSize;
        }
    }

    private void InstantiateSpawnHole()
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
            var potentialPosition = Utils.GetRandomVector(_minX, _maxX, _minY, _maxY);
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