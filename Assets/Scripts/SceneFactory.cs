using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Assets.Enums;


public class SceneFactory : MonoBehaviour
{

    public bool UseConfigFile;
    public GameObject Background, EnemyPrefab, AstroidPrefab, ItemPrefab;
    public int MaxNumberOfInnerAstroidsAllowed, NumberOfInnerAstroids, MaxNumberOfGameObjectsAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, MaxNumberOfItemsAllowed, NumberOfItems, SlotsOnGameGreedX, SlotsOnGameGreedY;
    
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
        InstantiateNewGameObject(NewGameObject.INNERASTROID, NumberOfInnerAstroids);
        InstantiateNewGameObject(NewGameObject.ENEMY, NumberOfEnemies);
        InstantiateNewGameObject(NewGameObject.ASTROID, NumberOfAstroids);
        //InstantiateNewGameObject(NewGameObject.ITEM, NumberOfItems);
    }

    private void InstantiateNewGameObject(NewGameObject ngo, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            int innerAstroidScale = GetInnerAstroidScale();
            Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(ngo, innerAstroidScale);
            if (ngo == NewGameObject.INNERASTROID)
            {
                GameObject innerAstroid = Instantiate(_gameObjectToPrefab[ngo],  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward)) as GameObject;
                innerAstroid.SendMessage("TheStart", innerAstroidScale);
            }
            else
            {
                Instantiate(_gameObjectToPrefab[ngo],  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
            }
        }
    }
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed(NewGameObject ngo, int innerAstroidScale = 0)
    {
        Vector2 randPointOnGreed;
        int edgesWidth = GetEdgesWidthByNewGameObject(ngo);
        if (ngo == NewGameObject.INNERASTROID)
        {
            //int innerAstroidScale = GetInnerAstroidScale();
            do{
                randPointOnGreed = GetNewRandomPointOnOneOfTheEdges(edgesWidth);
            } while (!IsThisPlaceFreeForTheInnerAstroid(randPointOnGreed, innerAstroidScale));
        }
        else if(ngo == NewGameObject.ASTROID)
        {
            do{
                randPointOnGreed = GetRandomPointOnTheOuterEdge();
            } while (!IsThisPlaceFree(randPointOnGreed));
        } else
        {
            do{
                randPointOnGreed = GetNewRandomPointOnOneOfTheEdges(edgesWidth);
            } while (!IsThisPlaceFree(randPointOnGreed));
        }

        return randPointOnGreed;
    }

    private bool IsThisPlaceFreeForTheInnerAstroid(Vector2 centerOfAstroid, int scaleOfAstroid)
    {
        for(int x = Mathf.Max(0, (int)centerOfAstroid.x - (scaleOfAstroid / 2)); (x < SlotsOnGameGreedX + 2) && (x < (int)centerOfAstroid.x + (scaleOfAstroid / 2)); x++)
            for(int y = Mathf.Max(0, (int)centerOfAstroid.y - (scaleOfAstroid / 2)); (y < SlotsOnGameGreedY + 2) && (y < (int)centerOfAstroid.y + (scaleOfAstroid / 2)); y++)
            {
                if(_gameObjectsOnGameGreed[x, y])
                    return false;
            }

        for(int x = Mathf.Max(0, (int)centerOfAstroid.x - (scaleOfAstroid / 2)); (x < SlotsOnGameGreedX + 2) && (x < (int)centerOfAstroid.x + (scaleOfAstroid / 2)); x++)
            for(int y = Mathf.Max(0, (int)centerOfAstroid.y - (scaleOfAstroid / 2)); (y < SlotsOnGameGreedY + 2) && (y < (int)centerOfAstroid.y + (scaleOfAstroid / 2)); y++)
            {
                Debug.Log("TAKEN - X:" + x + ", Y:" + y);
                _gameObjectsOnGameGreed[x, y] = true;
            }
        return true;
    }

    private bool IsThisPlaceFree(Vector2 pointOnGreed)
    {
        if(_gameObjectsOnGameGreed[(int)pointOnGreed.x, (int)pointOnGreed.y])
            return false;

        _gameObjectsOnGameGreed[(int)pointOnGreed.x, (int)pointOnGreed.y] = true;
        return true;
    }

    private int GetInnerAstroidScale()
    {
        return Random.Range(3, 10);
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
            case NewGameObject.INNERASTROID:
                return ((SlotsOnGameGreedY / 2) - 3);
            case NewGameObject.ENEMY:
                return 2; 
            case NewGameObject.ITEM:
                return ((SlotsOnGameGreedY / 2) - 2);
            case NewGameObject.ASTROID:
                return -1;
            default:
               throw new System.Exception("GameObject not supported"); 
        } 
    }

    private void ConfigureFromFile()
    {
            string[] props = {"MaxNumberOfInnerAstroidsAllowed", "MaxNumberOfGameObjectsAllowed", "MaxNumberOfEnemiesAllowed", "NumberOfInnerAstroids", "NumberOfEnemies", "MaxNumberOfAstroidsAllowed", "NumberOfAstroids", "MaxNumberOfItemsAllowed", "NumberOfItems", "SlotsOnGameGreedX", "SlotsOnGameGreedY"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberOfInnerAstroidsAllowed = (int)propsFromConfig["MaxNumberOfInnerAstroidsAllowed"];
            MaxNumberOfGameObjectsAllowed = (int)propsFromConfig["MaxNumberOfGameObjectsAllowed"];
            MaxNumberOfEnemiesAllowed = (int)propsFromConfig["MaxNumberOfEnemiesAllowed"];
            MaxNumberOfAstroidsAllowed = (int)propsFromConfig["MaxNumberOfAstroidsAllowed"];
            MaxNumberOfItemsAllowed = (int)propsFromConfig["MaxNumberOfItemsAllowed"];
            
            NumberOfInnerAstroids = (int)propsFromConfig["NumberOfInnerAstroids"];
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
            {NewGameObject.INNERASTROID, AstroidPrefab},
            {NewGameObject.ENEMY, EnemyPrefab},
            {NewGameObject.ASTROID, AstroidPrefab},
            {NewGameObject.ITEM, ItemPrefab}
        };
    }
}
