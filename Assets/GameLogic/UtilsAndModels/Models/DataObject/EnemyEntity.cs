using System.Collections.Generic;
using UnityEngine;

public class EnemyEntity
{
    public GameObject Prefab { get; private set; }
    public List<float> RandomSeededNumbers { get; private set; }

    public EnemyEntity(GameObject prefab, List<float> randomSeededNumbers)
    {
        Prefab = prefab;
        RandomSeededNumbers = randomSeededNumbers;
    }
}
