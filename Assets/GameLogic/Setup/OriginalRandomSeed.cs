using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System;

public class OriginalRandomSeed : MonoBehaviour
{
    private SeedfulRandomGenerator _seedfulRandomGenerator;

    void Awake()
    {
        int originalRandomSeed = SeedlessRandomGenerator.GetNumber();

        Debug.Log("Generated Original Random seed: " + originalRandomSeed);

        _seedfulRandomGenerator = new SeedfulRandomGenerator(originalRandomSeed);

        SetAllInitialSeedfulRandomGenerators();
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
        var itemOptions = itemsFactory.GetComponent<ItemOptions>();
        var itemRankPicker = itemsFactory.GetComponent<ItemRankPicker>();

        enemyBank.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        spawnHoleInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        asteroidExternalInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        asteroidInternalInstantiator.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        itemsBank.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        itemRankPicker.SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());

        var seeds = _seedfulRandomGenerator.GetRandomNumbersList(Enum.GetValues(typeof(ItemRank)).Length);
        itemOptions.SetAttackOptionsSeeds(seeds);

    }
}
