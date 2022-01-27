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
    private bool[,] _gameObjectsOnGameGreed;
    private Dictionary<NewGameObject,GameObject> _gameObjectToPrefab;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();

        SetGreedSizes();
        SetGameObjectToPrefabDictionary();
        InstantiateAllGameObjects();
    }

    private void InstantiateAllGameObjects()
    {
        InstantiateNewGameObject(NewGameObject.ENEMY, NumberOfEnemies);
        InstantiateNewGameObject(NewGameObject.ASTROID, NumberOfAstroids);
        InstantiateNewGameObject(NewGameObject.ITEM, NumberOfItems);
    }

    private void InstantiateNewGameObject(NewGameObject ngo, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(ngo);
            _gameObjectsOnGameGreed[(int)entryPointOnGreed.x, (int)entryPointOnGreed.y] = true;
            Instantiate(_gameObjectToPrefab[ngo],  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed(NewGameObject ngo)
    {
        Vector2 randPointOnGreed;
        if(ngo == NewGameObject.ASTROID)
        {
            do{
                randPointOnGreed = GetRandomPointOnTheOuterEdge();
            } while (_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        } else
        {
            int edgesWidth = GetEdgesWidthByNewGameObject(ngo);
            do{
                randPointOnGreed = GetNewRandomPointOnOneOfTheEdges(edgesWidth);
            } while (_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        }

        return randPointOnGreed;
    }

    public Vector2 GetNewRandomPointOnOneOfTheEdges(int edgesWidth)
    {
        Vector2 randPointOnGreed;

        do{
            switch(Random.Range(0,4))
            {
                case 0:
                    randPointOnGreed = Utils.GetRandomVector(
                        1, 1 + edgesWidth, 
                        1, SlotsOnGameGreedY);
                    break;
                case 1:
                    randPointOnGreed = Utils.GetRandomVector(
                        1, SlotsOnGameGreedX,
                        SlotsOnGameGreedY + 1 - edgesWidth, SlotsOnGameGreedY + 1);
                    break;
                case 2:
                    randPointOnGreed = Utils.GetRandomVector(
                        SlotsOnGameGreedX + 1 - edgesWidth, SlotsOnGameGreedX + 1, 
                        2, SlotsOnGameGreedY + 1);
                    break;
                case 3:
                    randPointOnGreed = Utils.GetRandomVector(
                        2, SlotsOnGameGreedX + 1, 
                        1, 1 + edgesWidth);
                    break;
                default:
                    throw new System.Exception("Not a random between 0 to 3, abort");   
            }
        } while (_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        
        return randPointOnGreed;
    }

    public Vector2 GetRandomPointOnTheOuterEdge()
    {
        switch(Random.Range(0,4))
        {
            case 0:
                return Utils.GetRandomVector(
                    0, 1, 
                    0, SlotsOnGameGreedY + 2);
            case 1:
                return Utils.GetRandomVector(
                    0, SlotsOnGameGreedX + 2, 
                    SlotsOnGameGreedY + 1, SlotsOnGameGreedY + 2);
            case 2:
                return Utils.GetRandomVector(
                    SlotsOnGameGreedX + 1, SlotsOnGameGreedX + 2, 
                    1, SlotsOnGameGreedY + 2);
            case 3:
                return Utils.GetRandomVector(
                    1, SlotsOnGameGreedX + 2, 
                    0, 1);
        }
        throw new System.Exception("Not a random between 0 to 3, abort");   
    }

    private int GetEdgesWidthByNewGameObject(NewGameObject ngo)
    {
        switch (ngo)
        {
            case NewGameObject.ENEMY:
                return 2; 
            case NewGameObject.ITEM:
                return ((SlotsOnGameGreedY / 2) - 2);
            case NewGameObject.ASTROID:
                throw new System.Exception("ASTROID is on the outer edge");
            default:
               throw new System.Exception("GameObject not supported"); 
        } 
    }

    private void ConfigureFromFile()
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
            
            if (TooManyGameObjects()) 
                throw new System.Exception("Number Of Enemies is wayyy too big, abort");
    }

    private bool TooManyGameObjects()
    {
        return 
            NumberOfEnemies > MaxNumberOfEnemiesAllowed || 
            NumberOfAstroids > MaxNumberOfAstroidsAllowed || 
            NumberOfItems > MaxNumberOfItemsAllowed ||
            (NumberOfEnemies + NumberOfAstroids + NumberOfItems) > MaxNumberOfGameObjectsAllowed;
    }

    private void SetGreedSizes()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;
        _xGreedSpacing = bgSize.x / SlotsOnGameGreedX;
        _yGreedSpacing = bgSize.y / SlotsOnGameGreedY;

        _bottomLeftPoint = new Vector3(bgSize.x / 2 * (-1) - _xGreedSpacing, bgSize.y / 2 * (-1) - _yGreedSpacing);
        _gameObjectsOnGameGreed  = new bool[SlotsOnGameGreedX + 2, SlotsOnGameGreedY + 2];
    }

    private void SetGameObjectToPrefabDictionary()
    {
        _gameObjectToPrefab = new Dictionary<NewGameObject, GameObject>()
        {
            {NewGameObject.ENEMY, EnemyPrefab},
            {NewGameObject.ASTROID, AstroidPrefab},
            {NewGameObject.ITEM, ItemPrefab}
        };
    }
}
