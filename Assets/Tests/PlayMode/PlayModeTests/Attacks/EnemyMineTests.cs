using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class EnemyMineTests
{
    private const float LIFETIME_MARGIN = 0.5f;
    private const float WAIT_FOR_HIT = 0.5f;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator MineStaysThenExpires()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var mine = TestUtils.LoadEnemyMine(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var lifetime = mine.GetComponent<BallShot>().GetLifeTime();
        AssertWrapper.Greater(lifetime, LIFETIME_MARGIN, "Mine lifetime is too short for this test", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(lifetime - LIFETIME_MARGIN);

        // THEN
        AssertWrapper.IsNotNull(mine, _currentSeed, "Mine vanished before its lifetime ended");
        AssertWrapper.AreEqual(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, (Vector2)mine.transform.position, "Mine moved", _currentSeed, 0.01f);

        // WHEN
        yield return new WaitForSeconds(2 * LIFETIME_MARGIN);

        // THEN
        AssertWrapper.IsTrue(mine == null, "Mine still exists after its lifetime ended", _currentSeed);
    }

    [UnityTest]
    public IEnumerator MineHurtsEnemyOnce()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(100);
        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var originalEnemyHealth = TestUtils.GetEnemyHealth();

        // WHEN
        var mine = TestUtils.LoadEnemyMine(TestUtils.GetEnemyPosition());
        var minePower = mine.GetComponent<AttackBehaviour>().AttackStats.Power;
        yield return new WaitForSeconds(WAIT_FOR_HIT);

        // THEN
        AssertWrapper.AreEqual(originalEnemyHealth - minePower, TestUtils.GetEnemyHealth(), "Mine did not hurt the enemy exactly once", _currentSeed);
        AssertWrapper.IsTrue(mine == null, "Mine still exists after hitting the enemy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AsteroidClearsMine()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(100);
        TestUtils.SetAsteroidPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var originalAsteroidHealth = TestUtils.GetAsteroidHealth();

        // WHEN
        var mine = TestUtils.LoadEnemyMine(TestUtils.GetAsteroid().transform.position);
        yield return new WaitForSeconds(WAIT_FOR_HIT);

        // THEN
        AssertWrapper.IsTrue(mine == null, "Mine still exists after touching the asteroid", _currentSeed);
        AssertWrapper.AreEqual(originalAsteroidHealth, TestUtils.GetAsteroidHealth(), "Mine hurt the asteroid", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
