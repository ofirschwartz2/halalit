using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Assets.Enums;


public class SceneFactory : MonoBehaviour
{

    public bool UseConfigFile;
    public GameObject Background, EnemyPrefab, AstroidPrefab, ItemPrefab;
    public int MaxNumberOfGameObjectsAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, MaxNumberOfItemsAllowed, NumberOfItems, SlotsOnGameGreedX, SlotsOnGameGreedY;
    
    private float _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 
    private bool[,] _gameObjectsOnGameGreed; //enemiesOnGameGreed
    //private NewGameObject _newGameObject;

    void Start()
    {
        if (UseConfigFile)
        {
            string[] props = {"MaxNumberOfGameObjectsAllowed", "MaxNumberOfEnemiesAllowed", "NumberOfEnemies", "MaxNumberOfAstroidsAllowed", "NumberOfAstroids", "MaxNumberOfItemsAllowed", "NumberOfItems", "SlotsOnGameGreedX", "SlotsOnGameGreedY"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberOfGameObjectsAllowed = (int)propsFromConfig["MaxNumberOfGameObjectsAllowed"];
            MaxNumberOfEnemiesAllowed = (int)propsFromConfig["MaxNumberOfEnemiesAllowed"];
            MaxNumberOfAstroidsAllowed = (int)propsFromConfig["MaxNumberOfAstroidsAllowed"];
            MaxNumberOfItemsAllowed = (int)propsFromConfig["MaxNumberOfItemsAllowed"];
            
            NumberOfEnemies = (int)propsFromConfig["NumberOfEnemies"];
            NumberOfAstroids = (int)propsFromConfig["NumberOfAstroids"];
            NumberOfItems = (int)propsFromConfig["NumberOfItems"];

            SlotsOnGameGreedX = (int)propsFromConfig["SlotsOnGameGreedX"];
            SlotsOnGameGreedY = (int)propsFromConfig["SlotsOnGameGreedY"];
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
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(NewGameObject.ENEMY);
            Instantiate(EnemyPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }

    private void InstantiateAllAstroids()
    {
        for (int i = 0; i < NumberOfAstroids; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(NewGameObject.ASTROID);
            Instantiate(AstroidPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    private void InstantiateAllItems()
    {
        for (int i = 0; i < NumberOfItems; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(NewGameObject.ITEM);
            Instantiate(ItemPrefab,  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed(NewGameObject ngo)
    {
        int edgesWidth;
        Vector2 randPointOnGreed;

        switch (ngo)
        {
            case NewGameObject.ENEMY:
                edgesWidth = 2;
            break;
            case NewGameObject.ASTROID:
                edgesWidth = 1;
            break;
            case NewGameObject.ITEM:
                edgesWidth = SlotsOnGameGreedY / 2 - 2;
            break;
            default:
               throw new System.Exception("GameObject not supported"); 
        } 

        do
        {
            randPointOnGreed = GetRandomPointOnOneOfTheEdges(edgesWidth);
        } while (_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        _gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y] = true;
        return randPointOnGreed;
    }


    public Vector2 GetRandomPointOnOneOfTheEdges(int edgesWidth)
    {
    switch(Random.Range(0,4))
        {
            case 0:
                return Utils.GetRandomVector(0, edgesWidth, 0, _gameObjectsOnGameGreed.GetLength(0));
            case 1:
                return Utils.GetRandomVector(_gameObjectsOnGameGreed.GetLength(0) - edgesWidth, _gameObjectsOnGameGreed.GetLength(0), 0, _gameObjectsOnGameGreed.GetLength(0));
            case 2:
                return Utils.GetRandomVector(0, _gameObjectsOnGameGreed.GetLength(0), 0, edgesWidth);
            case 3:
                return Utils.GetRandomVector(0, _gameObjectsOnGameGreed.GetLength(0), _gameObjectsOnGameGreed.GetLength(0) - edgesWidth, _gameObjectsOnGameGreed.GetLength(0));
        }
        throw new System.Exception("Not a random between 0 to 3, abort");   
    }

    private void SetGreedSizes()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;
        _bottomLeftPoint = new Vector3(bgSize.x / 2 * (-1), bgSize.y / 2 * (-1));

        _xGreedSpacing = bgSize.x / SlotsOnGameGreedX;
        _yGreedSpacing = bgSize.y / SlotsOnGameGreedY;

        _gameObjectsOnGameGreed  = new bool[SlotsOnGameGreedX,SlotsOnGameGreedY];
    }
}
