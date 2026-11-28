using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity
{
    public GameObject Prefab { get; private set; }
    public int Seed { get; private set; }
    // Tether Twins (#237): a paired entry makes two bodies, the second seeded with PartnerSeed
    public bool HasPartner { get; private set; }
    public int PartnerSeed { get; private set; }

    public EnemyEntity(GameObject prefab, int seed)
    {
        Prefab = prefab;
        Seed = seed;
    }

    public EnemyEntity(GameObject prefab, int seed, int partnerSeed) : this(prefab, seed)
    {
        HasPartner = true;
        PartnerSeed = partnerSeed;
    }
}
