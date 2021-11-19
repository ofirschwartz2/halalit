using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public int numberOfEnemies;
    public Vector3 screenTopLeft; 
    public float XGreedSpacing; 
    public float YGreedSpacing; 
    public float MaxNumberOfEnemies;
    public bool[,] enemiesOnGameGreed = new bool[10,10];
    public bool UseConfigFile;

    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = { "XGreedSpacing", "YGreedSpacing", "XScreenTopLeft", "YScreenTopLeft", "MaxNumberOfEnemies"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            XGreedSpacing = propsFromConfig["XGreedSpacing"];
            YGreedSpacing = propsFromConfig["YGreedSpacing"];
            MaxNumberOfEnemies = propsFromConfig["MaxNumberOfEnemies"];
            screenTopLeft = new Vector3(propsFromConfig["XScreenTopLeft"],propsFromConfig["YScreenTopLeft"]);
        }
        if (numberOfEnemies > MaxNumberOfEnemies) 
            throw new System.Exception("Number Of Enemies is wayyy too big, abort");
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector2 entryPointOnGreed = GetNewEnemyEntryPointOnGreed();
            Instantiate(EnemyPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return screenTopLeft + new Vector3(XGreedSpacing * xInGameGrid, (-1) * YGreedSpacing * yInGameGrid);
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
