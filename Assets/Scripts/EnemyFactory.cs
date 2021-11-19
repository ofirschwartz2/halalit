using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public float bottomRandom; // = -0.1f
    public float topRandom; // = 0.1f
    public int numberOfEnemies; // = 5
    public Vector3 screenTopLeft = new Vector3(-67f,37f);
    public float xGreedSpacing = 67f / 10f * 2f;
    public float yGreedSpacing = 37f / 10f * 2f;
    public bool[,] enemiesOnGameGreed = new bool[10,10];
    public int maxNumberOfEnemies = 30;
    void Start()
    {
        if (numberOfEnemies > maxNumberOfEnemies) 
            throw new System.Exception("Number Of Enemies is wayyy too big, abort");
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector2 entryPointOnGreed = GetNewEnemyEntryPointOnGreed();
            Instantiate(EnemyPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return screenTopLeft + new Vector3(xGreedSpacing * xInGameGrid, (-1) * yGreedSpacing * yInGameGrid);
    }

    public Vector2 GetNewEnemyEntryPointOnGreed()
    {
        Vector2 rand = GetRandomPointOnOneOfTheEdges();;
        while (enemiesOnGameGreed[(int)rand.x, (int)rand.y])
            rand = GetRandomPointOnOneOfTheEdges();
        enemiesOnGameGreed[(int)rand.x, (int)rand.y] = true;
        return rand;
    }

    public Vector2 GetRandomPointOnOneOfTheEdges()
    {
    switch(Random.Range(0,4))
        {
            case 0:
                return new Vector2(Random.Range(0,1),Random.Range(0,10));
            case 1:
                return new Vector2(Random.Range(9,10),Random.Range(0,10));
            case 2:
                return new Vector2(Random.Range(0,10),Random.Range(0,1));
            case 3:
                return new Vector2(Random.Range(0,10),Random.Range(9,10));
        }
        throw new System.Exception("Not a random between 0 to 3, abort");   
    }
}
