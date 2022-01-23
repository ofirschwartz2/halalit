using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class SceneFactory : MonoBehaviour
{
    public GameObject EnemyPrefab, AstroidsPrefab;
    public int MaxNumberAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, MaxNumberOfItemsAllowed, NumberOfItems;
    public bool[,] GameObjectsOnGameGreed = new bool[10,10]; //enemiesOnGameGreed
    public bool UseConfigFile;
    public GameObject Background;
    private float _leftGreedEdge, _bottomGreedEdge, _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MaxNumberOfEnemies", "NumberOfEnemies", "MaxNumberOfAstroids", "NumberOfAstroids", "MaxNumberOfItems", "NumberOfItems"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberAllowed = (int)propsFromConfig["MaxNumberAllowed"];
            MaxNumberOfEnemiesAllowed = (int)propsFromConfig["MaxNumberOfEnemies"];
            MaxNumberOfAstroidsAllowed = (int)propsFromConfig["MaxNumberOfAstroids"];
            MaxNumberOfItemsAllowed = (int)propsFromConfig["MaxNumberOfItems"];
            
            
            NumberOfEnemies = (int)propsFromConfig["NumberOfEnemies"];
            NumberOfAstroids = (int)propsFromConfig["NumberOfAstroids"];
            NumberOfItems = (int)propsFromConfig["NumberOfItems"];


        }

        if (IsTooMany()) 
            throw new System.Exception("Number Of Enemies is wayyy too big, abort");
        
        SetGreedSizes();
        InstantiateAllEnemies();
        InstantiateAllAstroids();
        InstantiateAllItems();
    }

    private bool IsTooMany()
    {
        return 
            NumberOfEnemies > MaxNumberOfEnemiesAllowed || 
            NumberOfAstroids > MaxNumberOfAstroidsAllowed || 
            NumberOfItems > MaxNumberOfItemsAllowed;
    }

    private void InstantiateAllEnemies()
    {
        for (int i = 0; i < NumberOfEnemies; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed();
            Instantiate(EnemyPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

    private void InstantiateAllAstroids()
    {
        for (int i = 0; i < NumberOfAstroids; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed();
            Instantiate(AstroidsPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    private void InstantiateAllItems(){} // TODO: ITEMS
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed()
    {
        Vector2 rand = GetRandomPointOnOneOfTheEdges();
        while (GameObjectsOnGameGreed[(int)rand.x, (int)rand.y])
            rand = GetRandomPointOnOneOfTheEdges();
        GameObjectsOnGameGreed[(int)rand.x, (int)rand.y] = true;
        return rand;
    }


    public Vector2 GetRandomPointOnOneOfTheEdges()
    {
    switch(Random.Range(0,4))
        {
            case 0:
                return Utils.GetRandomVector(0, 1, 0, GameObjectsOnGameGreed.GetLength(0));
            case 1:
                return Utils.GetRandomVector(GameObjectsOnGameGreed.GetLength(0) - 1, GameObjectsOnGameGreed.GetLength(0), 0, GameObjectsOnGameGreed.GetLength(0));
            case 2:
                return Utils.GetRandomVector(0, GameObjectsOnGameGreed.GetLength(0), 0, 1);
            case 3:
                return Utils.GetRandomVector(0, GameObjectsOnGameGreed.GetLength(0), GameObjectsOnGameGreed.GetLength(0) - 1, GameObjectsOnGameGreed.GetLength(0));
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
