using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Assets.Enums;


public class SceneFactory : MonoBehaviour
{

    public bool UseConfigFile;
    public GameObject Background, EnemyPrefab, AstroidPrefab, ItemPrefab;
    public int MaxNumberOfInnerAstroidsAllowed, NumberOfInnerAstroids, MaxNumberOfGameObjectsAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, MaxNumberOfItemsAllowed, NumberOfItems, SlotsOnGameGreedX, SlotsOnGameGreedY, InnerAstroidInfiniteLoopTH, InnerAstroidMinScale, InnerAstroidMaxScale;
    
    private float _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 
    private bool[,] _gameObjectsOnGameGreed;
    private Dictionary<NewGameObject,GameObject> _gameObjectToPrefab;
    private bool _stopCreatingInnerAstroids;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();

        SetGreedVariables();
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
            if (ngo == NewGameObject.INNERASTROID)
                if(_stopCreatingInnerAstroids)
                    return;
                else
                    InstantiateInnerAstroid();
            else
                Instantiate(_gameObjectToPrefab[ngo],  GetAbsolutePointOnGreed(GetNewEntryPointOnGreed(ngo)), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private void InstantiateInnerAstroid()
    {
        int innerAstroidScale = GetInnerAstroidScale();
        Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(NewGameObject.INNERASTROID, innerAstroidScale);
        if(_stopCreatingInnerAstroids)
            return;
        GameObject innerAstroid = Instantiate(_gameObjectToPrefab[NewGameObject.INNERASTROID], GetAbsolutePointOnGreed(entryPointOnGreed), Quaternion.AngleAxis(0, Vector3.forward)) as GameObject;
        innerAstroid.SendMessage("SetScale", innerAstroidScale);
        
        LockGreedByPointAndRadius(entryPointOnGreed, innerAstroidScale * innerAstroid.GetComponent<PolygonCollider2D>().bounds.size.x / 2);
        //Debug.Log("SIZE: " + innerAstroid.GetComponent<PolygonCollider2D>().bounds.size + ", EntryPointOnGreed: " + GetAbsolutePointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y) + ", Scale:" + innerAstroidScale);
    }

    private void LockGreedByPointAndRadius(Vector2 entryPointOnGreed, float radius)
    {
        float numerOfSlotsToLockFromEveryDirection = Mathf.Ceil(radius / _yGreedSpacing);
        
        for(int x = (int)Mathf.Max(0, entryPointOnGreed.x - numerOfSlotsToLockFromEveryDirection); (x < SlotsOnGameGreedX + 2) && (x <= entryPointOnGreed.x + numerOfSlotsToLockFromEveryDirection); x++)
            for(int y = (int)Mathf.Max(0, entryPointOnGreed.y - numerOfSlotsToLockFromEveryDirection); (y < SlotsOnGameGreedX + 2) && (y <= entryPointOnGreed.y + numerOfSlotsToLockFromEveryDirection); y++)
                _gameObjectsOnGameGreed[x,y] = true;
    }

    private Vector3 GetAbsolutePointOnGreed(Vector2 pointOnGreed) //(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * pointOnGreed.x + (_xGreedSpacing / 2), _yGreedSpacing * pointOnGreed.y + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed(NewGameObject ngo, int innerAstroidScale = 0)
    {
        Vector2 randPointOnGreed;
        if (ngo == NewGameObject.INNERASTROID)
        {
            return GetNewInnerAstroidEntryPointOnGreed(innerAstroidScale);
        }
        else if(ngo == NewGameObject.ASTROID)
        {

            do{
                randPointOnGreed = GetRandomPointOnTheOuterEdge();
            } while (!IsThisPlaceFree(randPointOnGreed));
        } else
        {
            do{
                randPointOnGreed = GetNewRandomPointOnOneOfTheEdges(GetEdgesWidthByNewGameObject(ngo));
            } while (!IsThisPlaceFree(randPointOnGreed));
        }

        return randPointOnGreed;
    }

    public Vector2 GetNewInnerAstroidEntryPointOnGreed(int innerAstroidScale)
    {
        Vector2 randPointOnGreed;
        int infiniteLoop = 0;
        do{
            if(++infiniteLoop > InnerAstroidInfiniteLoopTH)
            {
                _stopCreatingInnerAstroids = true;
                return Vector2.zero;
            }    
            randPointOnGreed = GetNewRandomPointOnScene();
        } while (!IsThisPlaceFreeForTheInnerAstroid(randPointOnGreed, innerAstroidScale));
        return randPointOnGreed;
    }

    private bool IsThisPlaceFreeForTheInnerAstroid(Vector2 centerOfAstroidOnGreed, int scaleOfAstroid)
    {
        float numerOfSlotsToLockFromEveryDirection = Mathf.Ceil(scaleOfAstroid / 2 / _yGreedSpacing);
        for(int x = (int)Mathf.Max(0, centerOfAstroidOnGreed.x - numerOfSlotsToLockFromEveryDirection); (x < SlotsOnGameGreedX + 2) && (x <= centerOfAstroidOnGreed.x + numerOfSlotsToLockFromEveryDirection); x++)
            for(int y = (int)Mathf.Max(0, centerOfAstroidOnGreed.y - numerOfSlotsToLockFromEveryDirection); (y < SlotsOnGameGreedY + 2) && (y <= centerOfAstroidOnGreed.y + numerOfSlotsToLockFromEveryDirection); y++)
            {
                if(_gameObjectsOnGameGreed[x, y])
                    return false;
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
        return Random.Range(InnerAstroidMinScale, InnerAstroidMaxScale);
    }

    public Vector2 GetNewRandomPointOnScene()
    {
        Vector2 randPointOnGreed;
        
        do{
            randPointOnGreed = Utils.GetRandomVector(
                1, SlotsOnGameGreedX + 1, 
                1, SlotsOnGameGreedY + 1);
        } while(_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        
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
            case NewGameObject.INNERASTROID:
                throw new System.Exception("INNERASTROID - GameObject not supported");
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
            string[] props = {"MaxNumberOfInnerAstroidsAllowed", "MaxNumberOfGameObjectsAllowed", "MaxNumberOfEnemiesAllowed", "NumberOfInnerAstroids", "NumberOfEnemies", "MaxNumberOfAstroidsAllowed", "NumberOfAstroids", "MaxNumberOfItemsAllowed", "NumberOfItems", "SlotsOnGameGreedX", "SlotsOnGameGreedY", "InnerAstroidInfiniteLoopTH", "InnerAstroidMinScale", "InnerAstroidMaxScale"};
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
            
            InnerAstroidInfiniteLoopTH = (int)propsFromConfig["InnerAstroidInfiniteLoopTH"];
            InnerAstroidMinScale = (int)propsFromConfig["InnerAstroidMinScale"]; 
            InnerAstroidMaxScale = (int)propsFromConfig["InnerAstroidMaxScale"];

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

    private void SetGreedVariables()
    {
        var bgSize = Background.GetComponent<Renderer>().bounds.size;
        _xGreedSpacing = bgSize.x / SlotsOnGameGreedX;
        _yGreedSpacing = bgSize.y / SlotsOnGameGreedY;

        _bottomLeftPoint = new Vector3(bgSize.x / 2 * (-1) - _xGreedSpacing, bgSize.y / 2 * (-1) - _yGreedSpacing);
        _gameObjectsOnGameGreed  = new bool[SlotsOnGameGreedX + 2, SlotsOnGameGreedY + 2];
        _stopCreatingInnerAstroids = false;
        LockGreedCenter();
    }

    private void LockGreedCenter()
    {
        for (int i = 5; i < 7; i++)
            for(int j = 5; j < 7; j++)
                _gameObjectsOnGameGreed[i,j] = true;
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
