using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float bottomRandom = -0.1f;
    public float topRandom = 0.1f;
    public int numberOfEnemies = 5;
    void Start()
    {
        for (int i=0; i<numberOfEnemies; i++)
        {
            Instantiate(EnemyPrefab,  new Vector3(Random.Range(bottomRandom, topRandom),Random.Range(bottomRandom, topRandom),0), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

}
