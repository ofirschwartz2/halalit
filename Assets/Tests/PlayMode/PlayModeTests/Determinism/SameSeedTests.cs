using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class SameSeedTests
{
    private const int FIXED_UPDATES_BEFORE_RECORDING_DROPS = 100;
    private const float DROP_POSITION_ACCEPTED_DELTA = 0.5f;

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator SameSeedSameEnemyOrder()
    {
        // GIVEN
        var firstRunEnemyOrder = GetEnemyOrder();

        // WHEN
        TestUtils.SetRandomSeed(_currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;

        var secondRunEnemyOrder = GetEnemyOrder();

        // THEN
        AssertWrapper.Greater(firstRunEnemyOrder.Count, 0, "No Enemies In The Enemy Bank", _currentSeed);
        AssertWrapper.AreEqual(firstRunEnemyOrder.Count, secondRunEnemyOrder.Count, "Different Number Of Enemies", _currentSeed);
        for (int i = 0; i < firstRunEnemyOrder.Count; i++)
        {
            AssertWrapper.AreEqual(firstRunEnemyOrder[i], secondRunEnemyOrder[i], "Different Enemy Order", _currentSeed);
        }
    }

    [UnityTest]
    public IEnumerator SameSeedSameDrops()
    {
        // GIVEN
        var firstRunDrops = new List<KeyValuePair<string, Vector2>>();
        yield return KillEnemiesAndRecordDrops(firstRunDrops);

        // WHEN
        TestUtils.SetRandomSeed(_currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;

        var secondRunDrops = new List<KeyValuePair<string, Vector2>>();
        yield return KillEnemiesAndRecordDrops(secondRunDrops);

        // THEN
        AssertWrapper.Greater(firstRunDrops.Count, 0, "Nothing Dropped", _currentSeed);
        AssertWrapper.AreEqual(firstRunDrops.Count, secondRunDrops.Count, "Different Number Of Drops", _currentSeed);
        foreach (var firstRunDrop in firstRunDrops)
        {
            var matchingDropIndex = secondRunDrops.FindIndex(secondRunDrop =>
                secondRunDrop.Key == firstRunDrop.Key &&
                Mathf.Abs(secondRunDrop.Value.x - firstRunDrop.Value.x) <= DROP_POSITION_ACCEPTED_DELTA &&
                Mathf.Abs(secondRunDrop.Value.y - firstRunDrop.Value.y) <= DROP_POSITION_ACCEPTED_DELTA);

            AssertWrapper.IsTrue(matchingDropIndex >= 0, "No Matching Drop For " + firstRunDrop.Key + " At " + firstRunDrop.Value, _currentSeed);
            secondRunDrops.RemoveAt(matchingDropIndex);
        }
    }

    private List<string> GetEnemyOrder()
    {
        var enemyBank = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription()).GetComponent<EnemyBank>();
        var enemyOrder = new List<string>();

        var spawnHoleEnemies = enemyBank.GetNextSpawnHoleEnemiesList();
        while (spawnHoleEnemies != null)
        {
            enemyOrder.Add("Group Size " + spawnHoleEnemies.Count);
            foreach (var enemyEntity in spawnHoleEnemies)
            {
                enemyOrder.Add(enemyEntity.Prefab.name + " " + enemyEntity.Seed);
            }

            spawnHoleEnemies = enemyBank.GetNextSpawnHoleEnemiesList();
        }

        return enemyOrder;
    }

    // Same steps as ValuablesTests.EnemySometimesDropValuable
    private IEnumerator KillEnemiesAndRecordDrops(List<KeyValuePair<string, Vector2>> drops)
    {
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

        do
        {
            weaponMovement.TryChangeWeaponPosition(Vector2.right);
            yield return null;

            weaponAttack.HumbleFixedUpdate(Vector2.right);
            yield return null;

            enemies = TestUtils.GetEnemies();
        } while (enemies.Length != 0);

        for (int i = 0; i < FIXED_UPDATES_BEFORE_RECORDING_DROPS; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        foreach (var valuable in TestUtils.GetValuables())
        {
            drops.Add(new KeyValuePair<string, Vector2>(valuable.name, valuable.transform.position));
        }
        foreach (var item in TestUtils.GetItems())
        {
            drops.Add(new KeyValuePair<string, Vector2>(item.name, item.transform.position));
        }
    }
}
