using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ShootingLaserAsteriskAttackTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    // A spawn hole keeps the enemy's scripts off until the enemy is out, so the attack's Start has not run (#290).
    // Here the attack and the state machine that drives it are off, as in the hole. Destroying the enemy then, as when a run ends,
    // must log no exception: Unity's test runner fails a test on any logged exception.
    [UnityTest]
    public IEnumerator DestroyedBeforeStartLogsNoError()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        enemy.GetComponent<MoveAimAttackStateMachine>().enabled = false;
        var attack = enemy.GetComponent<ShootingLaserAsteriskAttack>();
        attack.enabled = false;
        yield return null;
        // Unity calls OnDestroy only on an object that has been active, and Start only on a script that is on.
        AssertWrapper.IsTrue(enemy.activeInHierarchy, "Test setup: the laser asterisk is not active, so Unity would not call its OnDestroy", _currentSeed);
        AssertWrapper.IsFalse(attack.enabled, "Test setup: the attack was switched back on, so its Start may have run", _currentSeed);

        // WHEN
        Object.Destroy(enemy);
        yield return null;

        // THEN
        AssertWrapper.IsNull(enemy, _currentSeed, "The laser asterisk was not destroyed, so its OnDestroy did not run");
    }

    // A spawn hole switches off all the enemy's Behaviours except the few it keeps on (SpawnHole.DisableEnemyScripts), so most have not started.
    // Here all of them are off, so the test does not depend on which few the hole keeps on. Destroying the enemy then must log no exception.
    [UnityTest]
    public IEnumerator DestroyedBeforeAnyScriptStartsLogsNoError()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var behaviours = enemy.GetComponents<Behaviour>();
        foreach (var behaviour in behaviours)
        {
            behaviour.enabled = false;
        }
        yield return null;
        AssertWrapper.IsTrue(enemy.activeInHierarchy, "Test setup: the laser asterisk is not active, so Unity would not call its OnDestroy", _currentSeed);
        foreach (var behaviour in behaviours)
        {
            AssertWrapper.IsFalse(behaviour.enabled, $"Test setup: {behaviour.GetType().Name} on the laser asterisk was switched back on, so its Start may have run", _currentSeed);
        }

        // WHEN
        Object.Destroy(enemy);
        yield return null;

        // THEN
        AssertWrapper.IsNull(enemy, _currentSeed, "The laser asterisk was not destroyed, so its scripts' OnDestroy did not run");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
