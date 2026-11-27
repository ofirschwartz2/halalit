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
        enemy.GetComponent<ShootingLaserAsteriskAttack>().enabled = false;
        yield return null;

        // WHEN
        Object.Destroy(enemy);
        yield return null;

        // THEN
        AssertWrapper.IsNull(enemy, _currentSeed, "The laser asterisk was not destroyed, so its OnDestroy did not run");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
