using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class ValuablesTests
{

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator EnemyDrops1Or0Valuables()
    {
        // GIVEN
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        TestUtils.SetEnemiesHealth(1);
        yield return new WaitForSeconds(1);

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
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = TestUtils.GetShot();
        } while (shot != null);

        yield return new WaitForSeconds(1);

        // THEN
        AssertWrapper.IsNull(TestUtils.GetEnemy(), _currentSeed);
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
    }

    [UnityTest]
    public IEnumerator EnemySometimesDropValuable()
    {
        
        // GIVEN
        TestUtils.SetTestMode();

        var numberOfEnemies = 10;
        var positions = new List<Vector2>();
        for (int i = 0; i < numberOfEnemies; i++)
        {
            positions.Add(new Vector2((i * 1f) + 1f, 0f));
        }
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var loadedEnemies = objectLoader.LoadEnemiesInExternalSafeIsland(numberOfEnemies, _currentSeed);
        yield return null;

        foreach (var enemy in loadedEnemies)
        {
            if (enemy == null)
            {
                throw new System.Exception("Enemy is null");
            }
        }

        for (int i = 0; i < numberOfEnemies; i++)
        {
            TestUtils.SetGameObjectPosition(loadedEnemies[i], positions[i]);
        }
        TestUtils.SetEnemiesHealth(1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        yield return new WaitForSeconds(1);

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
        AssertWrapper.Greater(valuables.Length, 0, "No Valuables Dropped", _currentSeed);
        AssertWrapper.Greater(originalEnemiesCount, valuables.Length, "All dropped Valuables", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AsteroidDrops1Or0Valuables() 
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomAsteroidPosition();
        TestUtils.SetAsteroidsHealth(1);
        yield return new WaitForSeconds(1);

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
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = TestUtils.GetShot();
        } while (shot != null);

        yield return new WaitForSeconds(1);

        // THEN
        AssertWrapper.IsNull(TestUtils.GetAsteroid(), _currentSeed);
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
    }
    
    [UnityTest]
    public IEnumerator AsteroidSometimesDropValuable() 
    {
        // GIVEN
        var numberOfAsteroids = 10;
        var positions = new List<Vector2>()
        {
            new Vector2(3f, 0f),
            new Vector2(4f, 0f),
            new Vector2(5f, 0f),
            new Vector2(6f, 0f),
            new Vector2(7f, 0f),
            new Vector2(8f, 0f),
            new Vector2(9f, 0f),
            new Vector2(10f, 0f),
            new Vector2(11f, 0f),
            new Vector2(12f, 0f)
        };
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var loadedAsteroids = objectLoader.LoadAsteroidsInExternalSafeIsland(numberOfAsteroids, _currentSeed);
        yield return null;

        for (int i = 0; i < numberOfAsteroids; i++)
        {
            TestUtils.SetGameObjectPosition(loadedAsteroids[i], positions[i]);
        }

        TestUtils.SetAsteroidsHealth(1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        yield return new WaitForSeconds(1);

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
        AssertWrapper.Greater(valuables.Length, 0, "No Valuables Dropped", _currentSeed);
        AssertWrapper.Greater(originalAsteroidsCount, valuables.Length, "All dropped Valuables", _currentSeed);
    }
    
    [UnityTest]
    public IEnumerator ValuablesAddScore() 
    {
        // GIVEN
        var numberOfValuables = 3;
        var positions = new List<Vector2>()
        {
            new Vector2(1f, 0f),
            new Vector2(2f, 0f),
            new Vector2(3f, 0f)
        };
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        List<GameObject> valuables = objectLoader.LoadValuablesInExternalSafeIsland(numberOfValuables);
        yield return null;

        for (int i = 0; i < numberOfValuables; i++)
        {
            TestUtils.SetGameObjectPosition(valuables[i], positions[i]);
        }

        var halalitMovement = TestUtils.GetHalalitMovement();
        var valuableValues = TestUtils.GetValuableValues();
        var newValuables = TestUtils.GetValuables();
        var oldValuables = newValuables;
        var scoreHistory = new List<int>();
        yield return new WaitForSeconds(1);

        // WHEN
        do
        {
            var rightVector = Vector2.right;
            halalitMovement.TryMove(rightVector.x, rightVector.y, Time.deltaTime);
            newValuables = TestUtils.GetValuables();
            if (newValuables.Length < oldValuables.Length) 
            {
                scoreHistory.Add(TestUtils.GetScore());
                oldValuables = newValuables;
            }
            yield return null;

        } while (newValuables.Length != 0);

        // THEN
        AssertWrapper.AreEqual(scoreHistory[0], valuableValues.Find(valuableValue => valuableValue.Key == ValuableName.GOLD).Value, "Score Not Added Right");
        AssertWrapper.AreEqual(scoreHistory[1], scoreHistory[0] + valuableValues.Find(valuableValue => valuableValue.Key == ValuableName.GOLD).Value, "Score Not Added Right");
        AssertWrapper.AreEqual(scoreHistory[2], scoreHistory[1] + valuableValues.Find(valuableValue => valuableValue.Key == ValuableName.GOLD).Value, "Score Not Added Right");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}