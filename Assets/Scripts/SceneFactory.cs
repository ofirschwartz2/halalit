using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class SceneFactory : MonoBehaviour
{

    public bool UseConfigFile;
    public GameObject Background, EnemyPrefab, AstroidPrefab, ItemPrefab;
    public int MaxNumberOfGameObjectsAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, MaxNumberOfItemsAllowed, NumberOfItems, GameObjectsOnGameGreedX, GameObjectsOnGameGreedY;
    
    private float _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 
    private bool[,] _gameObjectsOnGameGreed; //enemiesOnGameGreed


    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MaxNumberOfGameObjectsAllowed", "MaxNumberOfEnemiesAllowed", "NumberOfEnemies", "MaxNumberOfAstroidsAllowed", "NumberOfAstroids", "MaxNumberOfItemsAllowed", "NumberOfItems", "GameObjectsOnGameGreedX", "GameObjectsOnGameGreedY"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberOfGameObjectsAllowed = (int)propsFromConfig["MaxNumberOfGameObjectsAllowed"];
            MaxNumberOfEnemiesAllowed = (int)propsFromConfig["MaxNumberOfEnemiesAllowed"];
            MaxNumberOfAstroidsAllowed = (int)propsFromConfig["MaxNumberOfAstroidsAllowed"];
            MaxNumberOfItemsAllowed = (int)propsFromConfig["MaxNumberOfItemsAllowed"];
            
            NumberOfEnemies = (int)propsFromConfig["NumberOfEnemies"];
            NumberOfAstroids = (int)propsFromConfig["NumberOfAstroids"];
            NumberOfItems = (int)propsFromConfig["NumberOfItems"];

            GameObjectsOnGameGreedX = (int)propsFromConfig["GameObjectsOnGameGreedX"];
            GameObjectsOnGameGreedY = (int)propsFromConfig["GameObjectsOnGameGreedY"];
        }

        if (TooManyGameObjects()) 
            throw new System.Exception("Number Of Enemies is wayyy too big, abort");
        
        SetGreedSizes();
        InstantiateAllEnemies();
        InstantiateAllAstroids();
        InstantiateAllItems();
    }

    private bool TooManyGameObjects()
    {
        return 
            NumberOfEnemies > MaxNumberOfEnemiesAllowed || 
            NumberOfAstroids > MaxNumberOfAstroidsAllowed || 
            NumberOfItems > MaxNumberOfItemsAllowed ||
            (NumberOfEnemies + NumberOfAstroids + NumberOfItems) > MaxNumberOfGameObjectsAllowed;
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
            Instantiate(AstroidPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    private void InstantiateAllItems()
    {
        for (int i = 0; i < NumberOfItems; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed();
            Instantiate(ItemPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed()
    {
        Vector2 randPointOnGreed = GetRandomPointOnOneOfTheEdges();
        while (_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y])
            randPointOnGreed = GetRandomPointOnOneOfTheEdges();
        _gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y] = true;
        return randPointOnGreed;
    }


    public Vector2 GetRandomPointOnOneOfTheEdges()
    {
    switch(Random.Range(0,4))
        {
            case 0:
                return Utils.GetRandomVector(0, 1, 0, _gameObjectsOnGameGreed.GetLength(0));
            case 1:
                return Utils.GetRandomVector(_gameObjectsOnGameGreed.GetLength(0) - 1, _gameObjectsOnGameGreed.GetLength(0), 0, _gameObjectsOnGameGreed.GetLength(0));
            case 2:
                return Utils.GetRandomVector(0, _gameObjectsOnGameGreed.GetLength(0), 0, 1);
            case 3:
                return Utils.GetRandomVector(0, _gameObjectsOnGameGreed.GetLength(0), _gameObjectsOnGameGreed.GetLength(0) - 1, _gameObjectsOnGameGreed.GetLength(0));
        }
        throw new System.Exception("Not a random between 0 to 3, abort");   
    }

    private void SetGreedSizes()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;
        _bottomLeftPoint = new Vector3(bgSize.x / 2 * (-1), bgSize.y / 2 * (-1));

        _xGreedSpacing = bgSize.x / GameObjectsOnGameGreedX;
        _yGreedSpacing = bgSize.y / GameObjectsOnGameGreedY;

        _gameObjectsOnGameGreed  = new bool[GameObjectsOnGameGreedX,GameObjectsOnGameGreedY];
    }
}
