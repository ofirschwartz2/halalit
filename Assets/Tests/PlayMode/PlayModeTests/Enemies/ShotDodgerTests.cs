using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ShotDodgerTests
{
    private const float WAIT_FOR_DODGE = 0.5f;
    private const float MIN_DODGE_DISTANCE = 0.8f;
    private const float MAX_STILL_DRIFT = 0.1f;
    private const float MAX_SIDEWAYS_IN_COOLDOWN = 0.2f;
    private const float MIN_COOLDOWN = 1f;
    private const int ENEMY_HEALTH = 100;
    private static readonly Vector2 SHOT_START_LEFT_OF_ENEMY = new(-2.5f, 0.3f);
    private static readonly Vector2 SHOT_START_RIGHT_OF_ENEMY = new(2.5f, 0.3f);
    private static readonly Vector2 SHOT_START_ABOVE_ENEMY = new(0.3f, 2.5f);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator DodgesBallShotAwayFromItsLine()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadDodgingEnemy(TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var startPosition = (Vector2)enemy.transform.position;

        // WHEN
        TestUtils.LoadBallShot(startPosition + SHOT_START_LEFT_OF_ENEMY, Vector2.right);
        yield return new WaitForSeconds(WAIT_FOR_DODGE);

        // THEN
        AssertWrapper.GreaterOrEqual(startPosition.y - MIN_DODGE_DISTANCE, enemy.transform.position.y, "Enemy did not dodge down, away from the shot's line", _currentSeed);
    }

    [UnityTest]
    public IEnumerator IgnoresShotFlyingAway()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadDodgingEnemy(TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var startPosition = (Vector2)enemy.transform.position;

        // WHEN
        TestUtils.LoadBallShot(startPosition + SHOT_START_RIGHT_OF_ENEMY, Vector2.right);
        yield return new WaitForSeconds(WAIT_FOR_DODGE);

        // THEN
        AssertWrapper.AreEqual(startPosition.y, enemy.transform.position.y, "Enemy dodged a shot that flies away from it", _currentSeed, MAX_STILL_DRIFT);
    }

    [UnityTest]
    public IEnumerator WaitsForCooldown()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadDodgingEnemy(TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        AssertWrapper.Greater(enemy.GetComponent<ShotDodger>().GetCooldown(), MIN_COOLDOWN, "Cooldown is too short for this test", _currentSeed);
        var startPosition = (Vector2)enemy.transform.position;

        TestUtils.LoadBallShot(startPosition + SHOT_START_LEFT_OF_ENEMY, Vector2.right);
        yield return new WaitForSeconds(WAIT_FOR_DODGE);
        AssertWrapper.GreaterOrEqual(startPosition.y - MIN_DODGE_DISTANCE, enemy.transform.position.y, "Enemy did not make the first dodge", _currentSeed);

        // WHEN
        // The first dodge still carries the enemy down, so the second shot comes from above: a second dodge would be sideways (along x).
        var positionAtSecondShot = (Vector2)enemy.transform.position;
        TestUtils.LoadBallShot(positionAtSecondShot + SHOT_START_ABOVE_ENEMY, Vector2.down);
        yield return new WaitForSeconds(WAIT_FOR_DODGE);

        // THEN
        AssertWrapper.AreEqual(positionAtSecondShot.x, enemy.transform.position.x, "Enemy dodged again during its cooldown", _currentSeed, MAX_SIDEWAYS_IN_COOLDOWN);
    }

    [UnityTest]
    public IEnumerator ShootingInRangeAndZigZagCarryShotDodger()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();

        // WHEN
        var shootingInRangeEnemy = TestUtils.LoadDodgingEnemy(TestUtils.SHOOTING_IN_RANGE_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var zigZagEnemy = TestUtils.LoadDodgingEnemy(TestUtils.ZIG_ZAG_ENEMY_PREFAB_PATH, -TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(shootingInRangeEnemy.GetComponent<ShotDodger>(), _currentSeed, "Shooting in range enemy has no ShotDodger");
        AssertWrapper.IsNotNull(zigZagEnemy.GetComponent<ShotDodger>(), _currentSeed, "ZigZag enemy has no ShotDodger");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
