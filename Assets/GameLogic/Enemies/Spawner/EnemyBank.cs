using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBank : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private List<KeyValuePair<GameObject, int>> _enemyPrefabsBank;
    [SerializeField]
    private int _minSpawnEnemyCount;
    [SerializeField]
    private int _maxSpawnEnemyCount;
    [SerializeField]
    private int _seededRandomNumbersPerEnemy;

    private List<EnemyEntity> _enemyEntityList;

    private List<int> _spawnHoleEnemyAmountsList;

    void Awake()
    {
        _enemyEntityList = new List<EnemyEntity>();
        _spawnHoleEnemyAmountsList = new List<int>();

        FillEnemyLists();
    }

    // Endless Ramp (#248): the first fill and every refill draw only from this bank's seeded generator, in the same order,
    // so the enemy order stays the same for everyone on a daily, through every refill.
    private void FillEnemyLists()
    {
        SetEnemiesList();
        SetSpawnHoleEnemyAmountsList();
    }

    private void SetEnemiesList()
    {
        _enemyEntityList = new List<EnemyEntity>();

        foreach (var enemyPrefab in _enemyPrefabsBank)
        {
            for (int i = 0; i < enemyPrefab.Value; i++)
            {
                _enemyEntityList.Add(new EnemyEntity(
                    enemyPrefab.Key,
                    _seedfulRandomGenerator.GetNumber()));
            }
        }

        _seedfulRandomGenerator.ShuffleList(_enemyEntityList);
    }

    private void SetSpawnHoleEnemyAmountsList()
    {
        var numberofEnemies = _enemyEntityList.Count;

        while (numberofEnemies > 0)
        {
            var amountOfEnemiesInSpawnHole = _seedfulRandomGenerator.Range(_minSpawnEnemyCount, _maxSpawnEnemyCount + 1);
            if (amountOfEnemiesInSpawnHole > numberofEnemies)
            {
                amountOfEnemiesInSpawnHole = numberofEnemies;
            }
            _spawnHoleEnemyAmountsList.Add(amountOfEnemiesInSpawnHole);
            numberofEnemies -= amountOfEnemiesInSpawnHole;
        }
    }

    public List<EnemyEntity> GetNextSpawnHoleEnemiesList()
    {
        if (_spawnHoleEnemyAmountsList.Count == 0) 
        {
            return null;
        }

        var enemiesToSpawn = _enemyEntityList.Take(_spawnHoleEnemyAmountsList.First()).ToList();
        _enemyEntityList.RemoveRange(0, _spawnHoleEnemyAmountsList.First());
        _spawnHoleEnemyAmountsList.RemoveAt(0);
        TryRefill();
        return enemiesToSpawn;
    }

    // Refills right after the last group is taken, not at the next request, so NoMoreEnemies() is never true between fills
    private void TryRefill()
    {
        if (_spawnHoleEnemyAmountsList.Count == 0)
        {
            FillEnemyLists();
        }
    }

    // True only when the scene's bank is empty: a bank with enemies refills itself (#248)
    public bool NoMoreEnemies()
    {
        return _spawnHoleEnemyAmountsList.Count == 0;
    }

#if UNITY_EDITOR
    internal int GetEnemiesPerFill()
    {
        return _enemyPrefabsBank.Sum(enemyPrefab => enemyPrefab.Value);
    }
#endif
}