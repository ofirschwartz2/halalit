using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

class ObjectLoaderTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator MoveHalalitToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        objectLoader.MoveHalalitToExternalSafeIsland();
        yield return null;

        // THEN
        bool IsHalalitInExternalSafeIsland = externalSafeIsland.transform.childCount == 1 && externalSafeIsland.transform.GetChild(0).CompareTag(Tag.HALALIT.GetDescription());

        AssertWrapper.IsTrue(IsHalalitInExternalSafeIsland, "Halalit was not moved into the external safe island", _currentSeed);
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "the scene has changed, maybe halalit died?", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoadEnemyToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        objectLoader.LoadEnemyInExternalSafeIsland();
        yield return null;

        // THEN
        GameObject loadedEnemy = externalSafeIsland.transform.GetChild(0).gameObject;
        bool IsEnemyInExternalSafeIsland = externalSafeIsland.transform.childCount == 1 && loadedEnemy.CompareTag(Tag.ENEMY.GetDescription());

        AssertWrapper.IsTrue(IsEnemyInExternalSafeIsland, "Enemy was not loaded into the external safe island", _currentSeed);
        AssertWrapper.AreEqual(Vector2.zero, loadedEnemy.GetComponent<Rigidbody2D>().velocity, "Enemy speed is not zero", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoadAsteroidToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        // THEN
        GameObject loadedAsteroid = externalSafeIsland.transform.GetChild(0).gameObject;
        bool IsAsteroidInExternalSafeIsland = externalSafeIsland.transform.childCount == 1 && loadedAsteroid.CompareTag(Tag.ASTEROID.GetDescription());

        AssertWrapper.IsTrue(IsAsteroidInExternalSafeIsland, "Asteroid was not loaded into the external safe island", _currentSeed);
        AssertWrapper.AreEqual(0, loadedAsteroid.GetComponent<AsteroidMovement>().GetSpeed(), "Asteroid speed is not zero", _currentSeed);
        AssertWrapper.AreEqual(Vector2.zero, loadedAsteroid.GetComponent<AsteroidMovement>().GetDirection(), "Asteroid direction is not zero", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoadItemToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        objectLoader.LoadItemInExternalSafeIsland();
        yield return null;

        // THEN
        GameObject loadedItem = externalSafeIsland.transform.GetChild(0).gameObject;
        bool IsItemInExternalSafeIsland = externalSafeIsland.transform.childCount == 1 && loadedItem.CompareTag(Tag.ITEM.GetDescription());

        AssertWrapper.IsTrue(IsItemInExternalSafeIsland, "Item was not loaded into the external safe island", _currentSeed);
        AssertWrapper.AreEqual(Vector2.zero, loadedItem.GetComponent<Rigidbody2D>().velocity, "Item velocity is not zero", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoadValuableToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        objectLoader.LoadValuableInExternalSafeIsland();
        yield return null;

        // THEN
        GameObject loadedValuable = externalSafeIsland.transform.GetChild(0).gameObject;
        bool IsValubaleInExternalSafeIsland = externalSafeIsland.transform.childCount == 1 && loadedValuable.CompareTag(Tag.VALUABLE.GetDescription());

        AssertWrapper.IsTrue(IsValubaleInExternalSafeIsland, "Valuable was not loaded into the external safe island", _currentSeed);
        AssertWrapper.AreEqual(Vector2.zero, loadedValuable.GetComponent<Rigidbody2D>().velocity, "Valuable velocity is not zero", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoadMultipleObjectsToExternalSafeIslandTest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var externalSafeIsland = GameObject.Find(TestUtils.EXTERNAL_SAFE_ISLAND_NAME);
        yield return null;

        // WHEN
        Collider2D halalitCollider = objectLoader.MoveHalalitToExternalSafeIsland().GetComponent<Collider2D>();
        Collider2D loadedEnemyCollider = objectLoader.LoadEnemyInExternalSafeIsland().GetComponent<Collider2D>();
        Collider2D loadedAsteroidCollider = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed).GetComponent<Collider2D>();
        Collider2D loadedItemCollider = objectLoader.LoadItemInExternalSafeIsland().GetComponent<Collider2D>(); 
        Collider2D loadedValuableCollider = objectLoader.LoadValuableInExternalSafeIsland().GetComponentInChildren<Collider2D>();
        yield return null;

        // THEN
        var allCollidersList = (new[] { halalitCollider, loadedEnemyCollider, loadedAsteroidCollider, loadedValuableCollider, loadedItemCollider }).ToList();

        AssertWrapper.AreEqual(5, externalSafeIsland.transform.childCount, "Not all game objects were loaded into the external safe island", _currentSeed);
        AssertWrapper.IsFalse(TestUtils.SomeCollidersTouch(allCollidersList), "Some loaded gameObjects touch each other", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}