using UnityEngine;

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
    private GameObject _itemBasePrefab;
    [SerializeField]
    private GameObject _valuableBasePrefab;
    [SerializeField]
    private float _loadedObjectsDistance;

    public GameObject MoveHalalitToExternalSafeIsland()
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_halalit);
        _halalit.transform.position = new(externalSafeIslandFreeLoadPosition.x, externalSafeIslandFreeLoadPosition.y, _halalit.transform.position.z);
        _halalit.transform.SetParent(_externalSafeIsland.transform);

        return _halalit;
    }

    public GameObject LoadEnemyInExternalSafeIsland()
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_enemyBasePrefab);
        GameObject loadedEnemy = Instantiate(_enemyBasePrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);

        return loadedEnemy;
    }

    public GameObject LoadAsteroidInExternalSafeIsland(int seed)
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_asteroidPrefab);
        GameObject loadedAsteroid = Instantiate(_asteroidPrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedAsteroid.GetComponent<AsteroidSharedBehavior>().SetInitialSeedfulRandomGenerator(seed);
        loadedAsteroid.GetComponent<AsteroidMovement>().SetSpeed(0);
        loadedAsteroid.GetComponent<AsteroidMovement>().SetDirection(Vector2.zero);

        return loadedAsteroid;
    }

    public GameObject LoadItemInExternalSafeIsland()
    {
        Vector2 externalSafeIslandFreeLoadPosition = GetExternalSafeIslandFreeLoadPosition(_itemBasePrefab);
        GameObject loadedItem = Instantiate(_itemBasePrefab, externalSafeIslandFreeLoadPosition, Quaternion.identity, _externalSafeIsland.transform);
        loadedItem.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

        return loadedItem;
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
    #endregion
}