using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OriginalRandomSeed : MonoBehaviour
{
    private SeedfulRandomGenerator _seedfulRandomGenerator;

    private void Awake()
    {
        
        int originalRandomSeed;

        originalRandomSeed = PlayerStats._isDailyRun ? (int)PlayerStats._dailySeed : SeedlessRandomGenerator.GetNumber();

        Debug.Log("Generated Original Random seed: " + originalRandomSeed);

        _seedfulRandomGenerator = new SeedfulRandomGenerator(originalRandomSeed);

        SetAllInitialSeedfulRandomGenerators();

#if UNITY_EDITOR
        if (SceneManager.GetActiveScene().name.Contains("Testing"))
#endif
            SetTestsInitialSeedfulRandomGenerators();
    }

    void SetAllInitialSeedfulRandomGenerators()
    {
        var enemiesSpawner = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription());
        var enemyBank = enemiesSpawner.GetComponent<EnemyBank>();
        var spawnHoleInstantiator = enemiesSpawner.GetComponent<SpawnHoleInstantiator>();
        var ansteroidInstantiator = GameObject.FindGameObjectWithTag(Tag.ASTEROID_INSTANTIATOR.GetDescription());
        var asteroidExternalInstantiator = ansteroidInstantiator.GetComponent<AsteroidExternalInstantiator>();
        var asteroidInternalInstantiator = ansteroidInstantiator.GetComponent<AsteroidInternalInstantiator>();
        var itemsFactory = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription());
        var itemsBank = itemsFactory.GetComponent<ItemsBank>();
        var itemRankPicker = itemsFactory.GetComponent<ItemRankPicker>();
        var itemDropper = itemsFactory.GetComponent<ItemDropper>();

        enemyBank.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        spawnHoleInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        asteroidExternalInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        asteroidInternalInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        itemsBank.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        itemRankPicker.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        itemDropper.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
    }

#if UNITY_EDITOR
    void SetTestsInitialSeedfulRandomGenerators()
    {
        var asteroids = GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
        foreach (var asteroid in asteroids)
        {
            var asteroidSharedBehavior = asteroid.GetComponent<AsteroidSharedBehavior>();
            asteroidSharedBehavior.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        }

        var enemies = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        foreach (var enemy in enemies)
        {
            var enemySharedBehavior = enemy.GetComponent<EnemySharedBehavior>();
            enemySharedBehavior.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        }
    }
#endif

}