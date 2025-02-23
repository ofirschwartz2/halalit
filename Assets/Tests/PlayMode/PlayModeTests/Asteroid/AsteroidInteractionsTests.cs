using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

class AsteroidInteractionsTests
{
    private Stopwatch _stopwatch;
    private int _currentSeed;

    public struct CornerCrushTestArguments
    {
        public Tag GameObjectToBeCrushedTag;
        public Func<GameObject, bool> IsGameObjectDestroyed;
        public Func<GameObject, bool> IsGameObjectExpectedToBeDestroyed;

        public CornerCrushTestArguments(Tag gameObjectToBeCrushedTag, Func<GameObject, bool> isGameObjectDestroyed, Func<GameObject, bool> isGameObjectexpectedToBeDestroyed)
        {
            GameObjectToBeCrushedTag = gameObjectToBeCrushedTag;
            IsGameObjectDestroyed = isGameObjectDestroyed;
            IsGameObjectExpectedToBeDestroyed = isGameObjectexpectedToBeDestroyed;
        }
    }

    private static readonly CornerCrushTestArguments[] _asteroidCrushGameObjectAtCornerTestInputs = new CornerCrushTestArguments[] {
        new CornerCrushTestArguments(Tag.HALALIT, halalit => TestUtils.IsGameOver(), halalit => Utils.IsCenterInsideTheWorld(halalit)),
        new CornerCrushTestArguments(Tag.ENEMY, enemyGameObject => enemyGameObject == null, enemy => Utils.IsCenterInsideTheWorld(enemy)),
        new CornerCrushTestArguments(Tag.ITEM, itemGameObject => itemGameObject == null, item => TestUtils.IsPartlyInsideTheWorld(item)),
        new CornerCrushTestArguments(Tag.VALUABLE, valuableGameObject => valuableGameObject == null, valuable => TestUtils.IsPartlyInsideTheWorld(valuable))
    };

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    [UnityTest]
    public IEnumerator AsteroidCrushGameObjectAtCornerTest([ValueSource(nameof(_asteroidCrushGameObjectAtCornerTestInputs))] CornerCrushTestArguments testArguments)
    {
        // GIVEN
        yield return null;
        LoadTestGameObjects(testArguments);
        var camera = GameObject.FindGameObjectWithTag(Tag.MAIN_CAMERA.GetDescription());
        var gameObjectToBeCrushed = GameObject.FindGameObjectWithTag(testArguments.GameObjectToBeCrushedTag.GetDescription());
        var asteroid = GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription()); 
        var asteroidKinematicMovement = asteroid.GetComponent<KinematicMovement>(); 
        var asteroidMovementSpeed = Resources.Load<GameObject>("Prefabs/Asteroids/Asteroid").GetComponent<AsteroidMovement>().GetSpeed();
        var asteroidMaxScale = GameObject.Find("AsteroidInstantiator").GetComponent<AsteroidInternalInstantiator>().GetAsteroidMaxScale();
        Vector2 nearLeftUpperCornerPosition = new(-17.5f, 8.5f);
        Vector2 nearLeftUpperCornerFartherPosition = new(-11f, 3.5f);
        TestUtils.SetTestMode();

        gameObjectToBeCrushed.transform.parent = null;
        gameObjectToBeCrushed.transform.position = nearLeftUpperCornerPosition;
        asteroid.transform.parent = null;
        asteroid.transform.position = nearLeftUpperCornerFartherPosition;
        asteroid.transform.localScale = new(asteroidMaxScale, asteroidMaxScale, 0);
        camera.GetComponent<CameraMovement>().SetGameObjectToFollow(gameObjectToBeCrushed);
        camera.transform.position = new(gameObjectToBeCrushed.transform.position.x, gameObjectToBeCrushed.transform.position.y, camera.transform.position.z);
        int timeoutInMilliseconds = 15000;
        yield return null;
         
        // WHEN
        asteroidKinematicMovement.SetDirection((gameObjectToBeCrushed.transform.position - asteroid.transform.position).normalized);
        asteroidKinematicMovement.SetSpeed(asteroidMovementSpeed);
        yield return null;

        while (gameObjectToBeCrushed != null && testArguments.IsGameObjectExpectedToBeDestroyed(gameObjectToBeCrushed))
        {
            if (_stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
            {
                AssertWrapper.Fail(timeoutInMilliseconds + " milliseconds Timeout");
            }
            
            yield return null;
        }
        for (int i = 0; i < 5; i++) // wait 5 more frames
        {
            yield return null;
        }

        // THEN
        AssertWrapper.IsTrue(testArguments.IsGameObjectDestroyed(gameObjectToBeCrushed), "The game object did not crush", _currentSeed);
    }

    private void LoadTestGameObjects(CornerCrushTestArguments testArguments)
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();

        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        LoadGameObejctToBeCrushed(objectLoader, testArguments.GameObjectToBeCrushedTag);
    }

    private void LoadGameObejctToBeCrushed(ObjectLoader objectLoader, Tag gameObjectToBeCrushedTag)
    {
        switch (gameObjectToBeCrushedTag)
        {
            case Tag.HALALIT:
                objectLoader.MoveHalalitToExternalSafeIsland();
                break;
            case Tag.ENEMY:
                objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
                break;
            case Tag.ITEM:
                objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
                break;
            case Tag.VALUABLE:
                objectLoader.LoadValuableInExternalSafeIsland();
                break;
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}