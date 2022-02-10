using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;
using Assets.Enums;

public class SceneFactory : MonoBehaviour
{
    public bool UseConfigFile;
    public GameObject Background, EnemyPrefab, AstroidPrefab;
    public int NumberOfInnerAstroids, MaxNumberOfGameObjectsAllowed, MaxNumberOfEnemiesAllowed, NumberOfEnemies, MaxNumberOfAstroidsAllowed, NumberOfAstroids, SlotsOnGameGreedX, SlotsOnGameGreedY, InfiniteLoopTH, InnerAstroidMinScale, InnerAstroidMaxScale;
    
    private float _xGreedSpacing, _yGreedSpacing;
    private Vector3 _bottomLeftPoint; 
    private bool[,] _gameObjectsOnGameGreed;
    private bool _stopCreatingInnerAstroids;

    void Start()
    {
        if (UseConfigFile)
            ConfigureFromFile();

        SetGreedVariables();
        InstantiateAllGameObjects();
    }

    private void InstantiateAllGameObjects()
    {
        InstantiateNewGameObject(new NewInnerAstroid(AstroidPrefab), NumberOfInnerAstroids);
        InstantiateNewGameObject(new NewEnemy(EnemyPrefab), NumberOfEnemies);
        InstantiateNewGameObject(new NewAstroid(AstroidPrefab), NumberOfAstroids);
    }

    private void InstantiateNewGameObject(INewGameObject newGameObject, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (newGameObject is NewInnerAstroid)
                if(_stopCreatingInnerAstroids)
                    return;
                else
                    InstantiateInnerAstroid((NewInnerAstroid)newGameObject);
            else
                Instantiate(newGameObject.GetPrefab(),  GetAbsolutePointOnGreed(GetNewEntryPointOnGreed(newGameObject)), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private void InstantiateInnerAstroid(NewInnerAstroid newInnerAstroid)
    {
        int innerAstroidScale = GetInnerAstroidScale();
        Vector2 entryPointOnGreed = GetNewEntryPointOnGreed(newInnerAstroid, innerAstroidScale);
        if(_stopCreatingInnerAstroids)
            return;
        
        GameObject innerAstroid = Instantiate(newInnerAstroid.GetPrefab(), GetAbsolutePointOnGreed(entryPointOnGreed), Quaternion.AngleAxis(0, Vector3.forward)) as GameObject;
        innerAstroid.SendMessage("SetScale", innerAstroidScale);
        innerAstroid.SendMessage("SetVelocity", false);

        LockGreedByPointAndRadius(entryPointOnGreed, innerAstroidScale * innerAstroid.GetComponent<PolygonCollider2D>().bounds.size.x / 2);
    }

    private void LockGreedByPointAndRadius(Vector2 entryPointOnGreed, float radius)
    {
        float numerOfSlotsToLockFromEveryDirection = Mathf.Ceil(radius / _yGreedSpacing);
        
        int fromXPoint = (int)Mathf.Max(0, entryPointOnGreed.x - numerOfSlotsToLockFromEveryDirection);
        int toXPoint = (int)Mathf.Min(SlotsOnGameGreedX + 2, entryPointOnGreed.x + numerOfSlotsToLockFromEveryDirection + 1);

        int fromYPoint = (int)Mathf.Max(0, entryPointOnGreed.y - numerOfSlotsToLockFromEveryDirection);
        int toYPoint = (int)Mathf.Min(SlotsOnGameGreedY + 2, entryPointOnGreed.y + numerOfSlotsToLockFromEveryDirection + 1); 

        for(int x = fromXPoint; x < toXPoint; x++)
            for(int y = fromYPoint; y < toYPoint; y++)
                _gameObjectsOnGameGreed[x,y] = true;
    } 

    private Vector3 GetAbsolutePointOnGreed(Vector2 pointOnGreed)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * pointOnGreed.x + (_xGreedSpacing / 2), _yGreedSpacing * pointOnGreed.y + (_yGreedSpacing / 2));
    }

    private Vector2 GetNewEntryPointOnGreed(INewGameObject newGameObject, int innerAstroidScale = 0)
    {
        int infiniteLoopBreak = 0;
        Vector2 randPointOnGreed;
        if (newGameObject is NewInnerAstroid)
        {
            return GetNewInnerAstroidEntryPointOnGreed(innerAstroidScale);
        }
        else if(newGameObject is NewAstroid)
        {
            do{
                if(++infiniteLoopBreak > InfiniteLoopTH)
                    throw new System.Exception("400 time trying to find a place without success");

                randPointOnGreed = GetRandomPointOnTheOuterEdge();
            } while (!IsThisPlaceFree(randPointOnGreed));
        } else
        {
            do{
                if(++infiniteLoopBreak > InfiniteLoopTH)
                    throw new System.Exception("400 time trying to find a place without success");  
                
                int? width = newGameObject.GetEdgeWidthForInstantiation();
                if(width == null)
                    throw new System.Exception("GetNewEntryPointOnGreed: NULL EXCEPTION");

                randPointOnGreed = GetNewRandomPointOnOneOfTheEdges((int)width);
            } while (!IsThisPlaceFree(randPointOnGreed));
        }

        return randPointOnGreed;
    }

    public Vector2 GetNewInnerAstroidEntryPointOnGreed(int innerAstroidScale)
    {
        Vector2 randPointOnGreed;
        int infiniteLoop = 0;
        do{
            if(++infiniteLoop > InfiniteLoopTH)
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

        int fromX = (int)Mathf.Max(0, centerOfAstroidOnGreed.x - numerOfSlotsToLockFromEveryDirection);
        int toX = (int)Mathf.Min(SlotsOnGameGreedX + 2, centerOfAstroidOnGreed.x + numerOfSlotsToLockFromEveryDirection + 1);

        int fromY = (int)Mathf.Max(0, centerOfAstroidOnGreed.y - numerOfSlotsToLockFromEveryDirection);
        int toY = (int)Mathf.Min(SlotsOnGameGreedY + 2, centerOfAstroidOnGreed.y + numerOfSlotsToLockFromEveryDirection + 1);

        for(int x = fromX; x < toX; x++)
            for(int y = fromY; y < toY; y++)
                if(_gameObjectsOnGameGreed[x, y])
                    return false;   
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
        int infiniteLoopBreak = 0;
        Vector2 randPointOnGreed;
        
        do{
            if(++infiniteLoopBreak > InfiniteLoopTH)
                    throw new System.Exception("400 time trying to find a place without success: GetNewRandomPointOnScene"); 

            randPointOnGreed = Utils.GetRandomVector(
                1, SlotsOnGameGreedX + 1, 
                1, SlotsOnGameGreedY + 1);
        } while(_gameObjectsOnGameGreed[(int)randPointOnGreed.x, (int)randPointOnGreed.y]);
        
        return randPointOnGreed;
    }

    public Vector2 GetNewRandomPointOnOneOfTheEdges(int edgesWidth)
    {
        int infiniteLoopBreak = 0;
        Vector2 randPointOnGreed;

        do{
            if(++infiniteLoopBreak > InfiniteLoopTH)
                throw new System.Exception("400 time trying to find a place without success: GetNewRandomPointOnOneOfTheEdges"); 
            switch(Random.Range(0,4))
            {
                case 0:
                    randPointOnGreed = Utils.GetRandomVector(1, 1 + edgesWidth, 1, SlotsOnGameGreedY);
                    break;
                case 1:
                    randPointOnGreed = Utils.GetRandomVector(1, SlotsOnGameGreedX,SlotsOnGameGreedY + 1 - edgesWidth, SlotsOnGameGreedY + 1);
                    break;
                case 2:
                    randPointOnGreed = Utils.GetRandomVector(SlotsOnGameGreedX + 1 - edgesWidth, SlotsOnGameGreedX + 1, 2, SlotsOnGameGreedY + 1);
                    break;
                case 3:
                    randPointOnGreed = Utils.GetRandomVector(2, SlotsOnGameGreedX + 1, 1, 1 + edgesWidth);
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
                return Utils.GetRandomVector(0, 1, 0, SlotsOnGameGreedY + 2);
            case 1:
                return Utils.GetRandomVector(0, SlotsOnGameGreedX + 2, SlotsOnGameGreedY + 1, SlotsOnGameGreedY + 2);
            case 2:
                return Utils.GetRandomVector(SlotsOnGameGreedX + 1, SlotsOnGameGreedX + 2, 1, SlotsOnGameGreedY + 2);
            case 3:
                return Utils.GetRandomVector(1, SlotsOnGameGreedX + 2, 0, 1);
            default:
                throw new System.Exception("Not a random between 0 to 3, abort");
        }
    }

    private bool TooManyGameObjects()
    {
        return 
            NumberOfEnemies > MaxNumberOfEnemiesAllowed || 
            NumberOfAstroids > MaxNumberOfAstroidsAllowed || 
            (NumberOfEnemies + NumberOfAstroids) > MaxNumberOfGameObjectsAllowed;
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
        for (int x = SlotsOnGameGreedX / 2; x < SlotsOnGameGreedX / 2 + 2; x++)
            for(int y = SlotsOnGameGreedY / 2; y < SlotsOnGameGreedY / 2 + 2; y++)
                _gameObjectsOnGameGreed[x,y] = true;
    }

    private void ConfigureFromFile()
    {
            string[] props = {"MaxNumberOfGameObjectsAllowed", "MaxNumberOfEnemiesAllowed", "NumberOfInnerAstroids", "NumberOfEnemies", "MaxNumberOfAstroidsAllowed", "NumberOfAstroids", "SlotsOnGameGreedX", "SlotsOnGameGreedY", "InfiniteLoopTH", "InnerAstroidMinScale", "InnerAstroidMaxScale"};
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            MaxNumberOfGameObjectsAllowed = (int)propsFromConfig["MaxNumberOfGameObjectsAllowed"];
            MaxNumberOfEnemiesAllowed = (int)propsFromConfig["MaxNumberOfEnemiesAllowed"];
            MaxNumberOfAstroidsAllowed = (int)propsFromConfig["MaxNumberOfAstroidsAllowed"];
            
            NumberOfInnerAstroids = (int)propsFromConfig["NumberOfInnerAstroids"];
            NumberOfEnemies = (int)propsFromConfig["NumberOfEnemies"];
            NumberOfAstroids = (int)propsFromConfig["NumberOfAstroids"];

            SlotsOnGameGreedX = (int)propsFromConfig["SlotsOnGameGreedX"];
            SlotsOnGameGreedY = (int)propsFromConfig["SlotsOnGameGreedY"];
            
            InfiniteLoopTH = (int)propsFromConfig["InfiniteLoopTH"];
            InnerAstroidMinScale = (int)propsFromConfig["InnerAstroidMinScale"]; 
            InnerAstroidMaxScale = (int)propsFromConfig["InnerAstroidMaxScale"];

            if (TooManyGameObjects()) 
                throw new System.Exception("Number Of Enemies is wayyy too big, abort");
    }
}
