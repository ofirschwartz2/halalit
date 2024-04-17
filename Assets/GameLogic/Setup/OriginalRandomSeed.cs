using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class OriginalRandomSeed : MonoBehaviour
{
    private SeedfulRandomGenerator seedfulRandomGenerator;

    void Start()
    {
        int originalRandomSeed = SeedlessRandomGenerator.GetNumber();

        Debug.Log("Generated Original Random seed: " + originalRandomSeed);

        seedfulRandomGenerator = new SeedfulRandomGenerator(originalRandomSeed);

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

        enemyBank.SetInitialSeedfulRandomGenerator(seedfulRandomGenerator.GetNumber());
        spawnHoleInstantiator.SetInitialSeedfulRandomGenerator(seedfulRandomGenerator.GetNumber());
        asteroidExternalInstantiator.SetInitialSeedfulRandomGenerator(seedfulRandomGenerator.GetNumber());
        asteroidInternalInstantiator.SetInitialSeedfulRandomGenerator(seedfulRandomGenerator.GetNumber());
    }
}
