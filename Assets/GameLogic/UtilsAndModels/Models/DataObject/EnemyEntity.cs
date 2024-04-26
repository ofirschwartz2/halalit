using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity
{
    public GameObject Prefab { get; private set; }
    public int Seed { get; private set; }

    public EnemyEntity(GameObject prefab, int seed)
    {
        Prefab = prefab;
        Seed = seed;
    }
}
