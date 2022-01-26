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
            Instantiate(_gameObjectToPrefab[ngo],  GetPointOnGreed(entryPointOnGreed.x,entryPointOnGreed.y), Quaternion.AngleAxis(0, Vector3.forward));
        }
    }
    
    private Vector3 GetPointOnGreed(float xInGameGrid, float yInGameGrid)
    {
        return _bottomLeftPoint + new Vector3(_xGreedSpacing * xInGameGrid + (_xGreedSpacing / 2), _yGreedSpacing * yInGameGrid + (_yGreedSpacing / 2));
    }

    public Vector2 GetNewEntryPointOnGreed(NewGameObject ngo)
    {
        int edgesWidth = GetEdgesWidthByNewGameObject(ngo);
        Vector2 randPointOnGreed;
        
        do{
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

    private int GetEdgesWidthByNewGameObject(NewGameObject ngo)
    {
        switch (ngo)
        {
            case NewGameObject.ENEMY:
                return 2;
            case NewGameObject.ASTROID:
                return 1;
            case NewGameObject.ITEM:
                return ((SlotsOnGameGreedY / 2) - 2);
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
        _bottomLeftPoint = new Vector3(bgSize.x / 2 * (-1), bgSize.y / 2 * (-1));

        _xGreedSpacing = bgSize.x / SlotsOnGameGreedX;
        _yGreedSpacing = bgSize.y / SlotsOnGameGreedY;

        _gameObjectsOnGameGreed  = new bool[SlotsOnGameGreedX,SlotsOnGameGreedY];
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
