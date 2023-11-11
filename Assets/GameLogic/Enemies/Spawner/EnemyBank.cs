using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBank : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<GameObject, int>> _enemyPrefabsBank;
    [SerializeField]
    private int _minSpawnEnemyCount;
    [SerializeField]
    private int _maxSpawnEnemyCount;

    private List<GameObject> _enemyPrefabList;
    private List<int> _spawnHoleEnemyAmountsList;

    void Awake()
    {
        _enemyPrefabList = new List<GameObject>();
        _spawnHoleEnemyAmountsList = new List<int>();

        SetEnemiesList();
        SetSpawnHoleEnemyAmountsList();
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

    private void SetSpawnHoleEnemyAmountsList()
    {
        var numberofEnemies = _enemyPrefabList.Count;

        while (numberofEnemies > 0)
        {
            var spawnHoleSize = Random.Range(_minSpawnEnemyCount, _maxSpawnEnemyCount + 1);
            if (spawnHoleSize > numberofEnemies)
            {
                spawnHoleSize = numberofEnemies;
            }
            _spawnHoleEnemyAmountsList.Add(spawnHoleSize);
            numberofEnemies -= spawnHoleSize;
        }
    }

    public List<GameObject> GetNextSpawnHoleEnemiesList()
    {
        if (_spawnHoleEnemyAmountsList.Count == 0) 
        {
            return null;
        }

        var enemiesToSpawn = _enemyPrefabList.Take(_spawnHoleEnemyAmountsList.First()).ToList();
        _enemyPrefabList.RemoveRange(0, _spawnHoleEnemyAmountsList.First());
        _spawnHoleEnemyAmountsList.RemoveAt(0);
        return enemiesToSpawn;
    }

    public bool NoMoreEnemies()
    {
        return _spawnHoleEnemyAmountsList.Count == 0;
    }
}