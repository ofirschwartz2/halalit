using UnityEngine;
using System.Collections.Generic;
using Assets.Enums;
internal class ObjectLoader : MonoBehaviour
{
    [SerializeField]
    private GameObject _externalSafeIsland;
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private GameObject _enemyBasePrefab;
    [SerializeField]
    private GameObject _asteroidPrefab;

    [SerializeField]
    private List<ItemTagToPrefab> _itemPrefabs;
    
    [SerializeField]
    private GameObject _valuableBasePrefab;

    [SerializeField]
    private float _loadedObjectsDistance;
    private System.Random _random;


    public GameObject MoveHalalitToExternalSafeIsland()
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_halalit);
        _halalit.transform.position = new(externalSafeIslandFreeLoadPosition.x, externalSafeIslandFreeLoadPosition.y, _halalit.transform.position.z);
        _halalit.transform.SetParent(_externalSafeIsland.transform);

        return _halalit;
    }

    public List<GameObject> LoadEnemiesInExternalSafeIsland(int enemiesCount, int seed)
    {
        InitializeRandomIfNeeded(seed);

        List<GameObject> loadedEnemies = new List<GameObject>();
        for (int i = 0; i < enemiesCount; i++)
        {
            var enemy = LoadEnemyInExternalSafeIsland(_random.Next());
            if (enemy == null)
            {
                throw new System.Exception("Enemy is null");
            }
            loadedEnemies.Add(enemy);
        }
        return loadedEnemies;
    }
    
    public GameObject LoadEnemyInExternalSafeIsland(int seed)
    {
        InitializeRandomIfNeeded(seed);

        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_enemyBasePrefab);
        GameObject loadedEnemy = Instantiate(_enemyBasePrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedEnemy.GetComponent<EnemySharedBehavior>().SetInitialSeedfulRandomGenerator(_random.Next());
        loadedEnemy.GetComponent<LameEnemy>().StopMovement();
        return loadedEnemy;
    }

    public List<GameObject> LoadAsteroidsInExternalSafeIsland(int asteroidsCount, int seed)
    {
        InitializeRandomIfNeeded(seed);

        List<GameObject> loadedAsteroids = new List<GameObject>();
        for (int i = 0; i < asteroidsCount; i++)
        {
            var asteroid = LoadAsteroidInExternalSafeIsland(_random.Next());
            if (asteroid == null)
            {
                throw new System.Exception("Asteroid is null");
            }
            loadedAsteroids.Add(asteroid);
        }
        return loadedAsteroids;
    }

    public GameObject LoadAsteroidInExternalSafeIsland(int seed)
    {
        InitializeRandomIfNeeded(seed);

        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_asteroidPrefab);
        GameObject loadedAsteroid = Instantiate(_asteroidPrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedAsteroid.GetComponent<AsteroidSharedBehavior>().SetInitialSeedfulRandomGenerator(_random.Next());
        loadedAsteroid.GetComponent<AsteroidMovement>().SetSpeed(0);
        loadedAsteroid.GetComponent<AsteroidMovement>().SetDirection(Vector2.zero);

        return loadedAsteroid;
    }

    public GameObject LoadItemInExternalSafeIsland(ItemName ItemName)
    {
        var itemPrefab = _itemPrefabs.Find(item => item.ItemName == ItemName).ItemPrefab;

        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(itemPrefab);
        GameObject loadedItem = Instantiate(itemPrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedItem.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        return loadedItem;
    }

    public List<GameObject> LoadValuablesInExternalSafeIsland(int valuablesCount)
    {
        List<GameObject> loadedValuables = new List<GameObject>();
        for (int i = 0; i < valuablesCount; i++)
        {
            loadedValuables.Add(LoadValuableInExternalSafeIsland());
        }
        return loadedValuables;
    }

    public GameObject LoadValuableInExternalSafeIsland()
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_valuableBasePrefab);
        GameObject loadedValuable = Instantiate(_valuableBasePrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedValuable.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        return loadedValuable;
    }

    #region Utils
    private Vector2 GetExternalSafeIslandFreeLoadPosition(GameObject gameObjectToLoad)
    {
        Vector2 lowestFreeGameObjectLoadingPosition = GetTheLowestFreeGameObjectLoadingPosition();
        Bounds externalSafeIslandBounds = _externalSafeIsland.GetComponent<BoxCollider2D>().bounds;
        Vector2 gameObjectToLoadSize = gameObjectToLoad.GetComponent<SpriteRenderer>().size * gameObjectToLoad.transform.localScale;
        float gameObjectToLoadHalfYSize = gameObjectToLoadSize.y / 2;

        return new(externalSafeIslandBounds.center.x, lowestFreeGameObjectLoadingPosition.y - _loadedObjectsDistance - gameObjectToLoadHalfYSize);
    }

    private Vector2 GetTheLowestFreeGameObjectLoadingPosition()
    {
        GameObject lowestLoadedGameObject = null;
        float lowestLoadedGameObjectY = float.MaxValue;

        foreach (Transform loadedGameObject in _externalSafeIsland.transform)
        {
            if (loadedGameObject.position.y < lowestLoadedGameObjectY)
            {
                lowestLoadedGameObjectY = loadedGameObject.position.y;
                lowestLoadedGameObject = loadedGameObject.gameObject;
            }
        }

        if (lowestLoadedGameObject != null)
        {
            Vector2 lowestLoadedGameObjectSize = lowestLoadedGameObject.GetComponent<SpriteRenderer>().size * lowestLoadedGameObject.transform.localScale;
            float lowestLoadedGameObjectHalfYSize = lowestLoadedGameObjectSize.y / 2;
            return new(lowestLoadedGameObject.transform.position.x, lowestLoadedGameObject.transform.position.y - lowestLoadedGameObjectHalfYSize);
        }
        else
        {
            Bounds externalSafeIslandBounds = _externalSafeIsland.GetComponent<BoxCollider2D>().bounds;
            return new(externalSafeIslandBounds.center.x, externalSafeIslandBounds.max.y);
        }
    }

    private void InitializeRandomIfNeeded(int seed)
    {
        if (_random == null)
        {
            _random = new System.Random(seed);
        }
    }
    #endregion
}