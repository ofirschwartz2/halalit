using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyFactory : MonoBehaviour
{
    public GameObject EnemyPrefab;
    public int NumberOfEnemies;
    public float MaxNumberOfEnemies;
    public bool[,] enemiesOnGameGreed = new bool[10,10];
    public bool UseConfigFile;
    public GameObject Background;
    private float _leftGreedEdge, _bottomGreedEdge, _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MaxNumberOfEnemies", "NumberOfEnemies"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberOfEnemies = propsFromConfig["MaxNumberOfEnemies"];
            NumberOfEnemies = (int)propsFromConfig["NumberOfEnemies"];
        }

        if (NumberOfEnemies > MaxNumberOfEnemies) 
            throw new System.Exception("Number Of Enemies is wayyy too big, abort");
        
        SetGreedSizes();
        InstantiateAllEnemies();
    }

    private void InstantiateAllEnemies()
    {
        for (int i = 0; i < NumberOfEnemies; i++)
        {
            Vector2 entryPointOnGreed = GetNewEnemyEntryPointOnGreed();
            Instantiate(EnemyPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEnemyEntryPointOnGreed()
    {
        Vector2 rand = GetRandomPointOnOneOfTheEdges();
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
                return Utils.GetRandomVector(0, 1, 0, enemiesOnGameGreed.GetLength(0));
            case 1:
                return Utils.GetRandomVector(enemiesOnGameGreed.GetLength(0) - 1, enemiesOnGameGreed.GetLength(0), 0, enemiesOnGameGreed.GetLength(0));
            case 2:
                return Utils.GetRandomVector(0, enemiesOnGameGreed.GetLength(0), 0, 1);
            case 3:
                return Utils.GetRandomVector(0, enemiesOnGameGreed.GetLength(0), enemiesOnGameGreed.GetLength(0) - 1, enemiesOnGameGreed.GetLength(0));
        }
        throw new System.Exception("Not a random between 0 to 3, abort");   
    }

    private void SetGreedSizes()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;

        _leftGreedEdge = bgSize.x / 2 * (-1);
        _bottomGreedEdge = bgSize.y / 2 * (-1);
        _bottomLeftPoint = new Vector3(_leftGreedEdge, _bottomGreedEdge);
        _xGreedSpacing = bgSize.x / 10;
        _yGreedSpacing = bgSize.y / 10;
    }
}
