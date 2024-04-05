using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class ValuablesTests
{

    private const string SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    private const string SCENE_WITH_MANY_ENEMIES_FROM_RIGHT_NAME = "TestingWithManyEnemiesFromRight";
    private const string SCENE_WITH_ASTEROID_NAME = "TestingWithAsteroid";
    private const string SCENE_WITH_MANY_ASTEROIDS_FROM_RIGHT_NAME = "TestingWithManyAsteroidsFromRight";


    [SetUp]
    public void SetUp()
    {

        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName)
        {
            case FUNCTION_ENEMY_DROPS_1_OR_0_VALUABLES_NAME:
                SceneManager.LoadScene(SCENE_WITH_ENEMY_NAME);
                break;

            case FUNCTION_ENEMY_SOMETIMES_DROPS_VALUABLE_NAME:
                SceneManager.LoadScene(SCENE_WITH_MANY_ENEMIES_FROM_RIGHT_NAME);
                break;
                
            case FUNCTION_ASTEROID_DROPS_1_OR_0_VALUABLES_NAME:
                SceneManager.LoadScene(SCENE_WITH_ASTEROID_NAME);
                break;

            case FUNCTION_ASTEROID_SOMETIMES_DROPS_VALUABLE_NAME:
                SceneManager.LoadScene(SCENE_WITH_MANY_ASTEROIDS_FROM_RIGHT_NAME);
                break;
                
            default:
                throw new System.Exception("No scene for test: " + testName);
        }
    }

    private const string FUNCTION_ENEMY_DROPS_1_OR_0_VALUABLES_NAME = "EnemyDrops1Or0Valuables";
    [UnityTest]
    public IEnumerator EnemyDrops1Or0Valuables()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetEnemiesSeededNumbers();
        TestUtils.SetEnemiesHealth(1);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = TestUtils.GetShot();
        } while (shot != null);

        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsNull(TestUtils.GetEnemy(), seed);
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", seed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", seed, acceptedDelta);
    }

    private const string FUNCTION_ENEMY_SOMETIMES_DROPS_VALUABLE_NAME = "EnemySometimesDropValuable";
    [UnityTest]
    public IEnumerator EnemySometimesDropValuable()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetEnemiesSeededNumbers();
        TestUtils.SetEnemiesHealth(1);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        yield return null;

        var enemies = TestUtils.GetEnemies();
        var originalEnemiesCount = enemies.Length;

        // WHEN
        do
        {
            weaponMovement.TryChangeWeaponPosition(Vector2.right);
            yield return null;

            weaponAttack.HumbleFixedUpdate(Vector2.right);
            yield return null;

            enemies = TestUtils.GetEnemies();
        } while (enemies.Length != 0);

        // THEN
        var valuables = TestUtils.GetValuables();
        AssertWrapper.Greater(valuables.Length, 0, "No Valuables Dropped", seed);
        AssertWrapper.Greater(originalEnemiesCount, valuables.Length, "All dropped Valuables", seed);
    }

    
    private const string FUNCTION_ASTEROID_DROPS_1_OR_0_VALUABLES_NAME = "AsteroidDrops1Or0Valuables";
    [UnityTest]
    public IEnumerator AsteroidDrops1Or0Valuables() 
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        //TestUtils.SetAsteroidsSeededNumbers(); // TODO: Fix this
        TestUtils.SetAsteroidsHealth(1);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomAsteroidPosition();
        yield return null;

        var targetClosestPosition = TestUtils.GetAsteroidNearestPositionToHalalit();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = TestUtils.GetShot();
        } while (shot != null);

        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsNull(TestUtils.GetAsteroid(), seed);
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", seed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", seed, acceptedDelta);
    }

    private const string FUNCTION_ASTEROID_SOMETIMES_DROPS_VALUABLE_NAME = "AsteroidSometimesDropValuable";
    [UnityTest]
    public IEnumerator AsteroidSometimesDropValuable() 
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        //TestUtils.SetAsteroidsSeededNumbers(); // TODO: Fix this
        TestUtils.SetAsteroidsHealth(1);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        yield return null;

        var asteroids = TestUtils.GetAsteroids();
        var originalAsteroidsCount = asteroids.Length;

        // WHEN
        do
        {
            weaponMovement.TryChangeWeaponPosition(Vector2.right);
            yield return null;

            weaponAttack.HumbleFixedUpdate(Vector2.right);
            yield return null;

            asteroids = TestUtils.GetAsteroids();
        } while (asteroids.Length != 0);

        // THEN
        var valuables = TestUtils.GetValuables();
        AssertWrapper.Greater(valuables.Length, 0, "No Valuables Dropped", seed);
        AssertWrapper.Greater(originalAsteroidsCount, valuables.Length, "All dropped Valuables", seed);
    }
    
    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}