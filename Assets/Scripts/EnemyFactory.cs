using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    //public int numberOfEnemies = 5;
    //public float[,] positions = new float[3][numberOfEnemies]; { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
    //public float[,] rotaions = new float[4,numberOfEnemies];


    void Start()
    {
            float bottomRandom=-20f;
            float topRandom=20f;
            Instantiate(EnemyPrefab,  new Vector3(Random.Range(bottomRandom, topRandom),Random.Range(bottomRandom, topRandom)), Quaternion.AngleAxis(0, Vector3.forward));
            Instantiate(EnemyPrefab,  new Vector3(Random.Range(bottomRandom, topRandom),Random.Range(bottomRandom, topRandom)), Quaternion.AngleAxis(0, Vector3.forward));
    }

}
