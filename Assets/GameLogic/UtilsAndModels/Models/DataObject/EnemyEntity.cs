using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity
{
    public GameObject Prefab { get; private set; }
    public int RandomSeededNumber { get; private set; }

    public EnemyEntity(GameObject prefab, int randomSeededNumber)
    {
        Prefab = prefab;
        RandomSeededNumber = randomSeededNumber;
    }
}
