using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float bottomRandom; // = -0.1f
    public float topRandom; // = 0.1f
    public int numberOfEnemies; // = 5
    void Start()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Instantiate(EnemyPrefab,  new Vector2(Random.Range(bottomRandom, topRandom), Random.Range(bottomRandom, topRandom)), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

}
