using Assets.Enums;
using Assets.Models;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class WoundedRetreatTests
{
    private const int ENEMY_HEALTH = 7;
    private const int HALALIT_HEALTH = 1000;
    private const float FLEE_CHECK_TIME = 1f;
    private const float MIN_FLEE_DISTANCE = 0.5f;
    private const float RETREAT_END_MARGIN = 0.5f;
    private const float DISTANCE_FROM_EDGE = 1f;
    private const float MAX_WAIT_FOR_ATTACK = 30f;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator EveryEnemyPrefabHasWoundedRetreat()
    {
        // GIVEN
        yield return null;

        // WHEN
        var prefabs = Resources.LoadAll<GameObject>(TestUtils.ENEMIES_PREFABS_PATH);

        // THEN
        foreach (var prefab in prefabs)
        {
            if (prefab.GetComponent<EnemySharedBehavior>() == null)
            {
                continue;
            }

            AssertWrapper.IsNotNull(prefab.GetComponent<WoundedRetreat>(), _currentSeed, $"{prefab.name} has no WoundedRetreat (Needs Ofir in Unity, #201)");
            foreach (var pausable in prefab.GetComponentsInChildren<IPausableEnemyBehaviour>(true))
            {
                AssertWrapper.AreSame(prefab, ((Component)pausable).gameObject, $"{prefab.name} has a {pausable.GetType().Name} below its root, where WoundedRetreat cannot pause it", _currentSeed);
            }
        }
    }

    [UnityTest]
    public IEnumerator WoundingHitStartsRetreat()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;

        // WHEN
        Wound(enemy, ENEMY_HEALTH - woundedRetreat.GetWoundedHealth());
        yield return new WaitForFixedUpdate();
        var distanceAtWound = Vector2.Distance(halalitPosition, enemy.transform.position);
        yield return new WaitForSeconds(FLEE_CHECK_TIME);

        // THEN
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "Wounding hit did not start the retreat", _currentSeed);
        AssertWrapper.IsFalse(enemy.GetComponent<LameEnemy>().enabled, "Enemy kept driving itself during the retreat", _currentSeed);
        AssertWrapper.Greater(Vector2.Distance(halalitPosition, enemy.transform.position), distanceAtWound + MIN_FLEE_DISTANCE, "Retreating enemy did not move away from the Halalit", _currentSeed);
    }

    [UnityTest]
    public IEnumerator HitAboveThresholdNoRetreat()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        AssertWrapper.Greater(ENEMY_HEALTH - 1, woundedRetreat.GetWoundedHealth(), "Wounded health is too high for this test", _currentSeed);

        // WHEN
        Wound(enemy, 1);
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.IsFalse(woundedRetreat.IsRetreating(), "A hit above the wounded health started the retreat", _currentSeed);
        AssertWrapper.IsTrue(enemy.GetComponent<LameEnemy>().enabled, "A hit above the wounded health paused the enemy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator RetreatEndsHealed()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var expectedHealth = Mathf.Min(woundedRetreat.GetHealTo(), ENEMY_HEALTH);

        // WHEN
        Wound(enemy, ENEMY_HEALTH - woundedRetreat.GetWoundedHealth());
        yield return new WaitForSeconds(woundedRetreat.GetRetreatDuration() + RETREAT_END_MARGIN);

        // THEN
        AssertWrapper.IsFalse(woundedRetreat.IsRetreating(), "Retreat did not end after its duration", _currentSeed);
        AssertWrapper.AreEqual(expectedHealth, TestUtils.GetEnemyHealth(), "Enemy did not heal at the end of the retreat", _currentSeed);
        AssertWrapper.IsTrue(enemy.GetComponent<LameEnemy>().enabled, "Enemy's driver was not switched back on after the retreat", _currentSeed);
    }

    [UnityTest]
    public IEnumerator RetreatOnlyOnce()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        Wound(enemy, ENEMY_HEALTH - woundedRetreat.GetWoundedHealth());
        yield return new WaitForSeconds(woundedRetreat.GetRetreatDuration() + RETREAT_END_MARGIN);
        AssertWrapper.IsFalse(woundedRetreat.IsRetreating(), "First retreat did not end", _currentSeed);
        AssertWrapper.Greater(TestUtils.GetEnemyHealth(), woundedRetreat.GetWoundedHealth(), "Enemy did not heal after the first retreat, so a second wound cannot be checked", _currentSeed);

        // WHEN
        Wound(enemy, (int)TestUtils.GetEnemyHealth() - woundedRetreat.GetWoundedHealth());
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.AreEqual(woundedRetreat.GetWoundedHealth(), TestUtils.GetEnemyHealth(), "Second wound did not land", _currentSeed);
        AssertWrapper.IsFalse(woundedRetreat.IsRetreating(), "Enemy retreated a second time", _currentSeed);
        AssertWrapper.IsTrue(enemy.GetComponent<LameEnemy>().enabled, "Second wound paused the enemy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CorneredEndsWithoutHeal()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        var rightEdgeInnerX = TestUtils.GetExternalRightEdge().GetComponent<Collider2D>().bounds.min.x;
        TestUtils.SetEnemyPosition(new Vector2(rightEdgeInnerX - DISTANCE_FROM_EDGE, TestUtils.GetHalalit().transform.position.y));
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var woundedHealth = woundedRetreat.GetWoundedHealth();
        var retreatStartTime = Time.time;

        // WHEN
        Wound(enemy, ENEMY_HEALTH - woundedHealth);
        while (woundedRetreat.IsRetreating() && Time.time < retreatStartTime + woundedRetreat.GetRetreatDuration())
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsFalse(woundedRetreat.IsRetreating(), "Cornered enemy kept retreating for the whole duration", _currentSeed);
        AssertWrapper.Less(Time.time - retreatStartTime, woundedRetreat.GetRetreatDuration(), "Cornered retreat did not end early", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(woundedRetreat.GetRetreatDuration() + RETREAT_END_MARGIN);

        // THEN
        AssertWrapper.AreEqual(woundedHealth, TestUtils.GetEnemyHealth(), "Cornered enemy healed", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LaserAsteriskStopsFiring()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = TestUtils.LoadEnemy(TestUtils.SHOOTING_LASER_ASTERISK_ENEMY_PREFAB_PATH, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        TestUtils.GetHalalit().GetComponent<Health>().SetHealth(HALALIT_HEALTH);

        var waitStartTime = Time.time;
        while (TestUtils.GetEnemyLaserBeams().Count == 0 && Time.time < waitStartTime + MAX_WAIT_FOR_ATTACK)
        {
            yield return new WaitForFixedUpdate();
        }
        AssertWrapper.Greater(TestUtils.GetEnemyLaserBeams().Count, 0, "Laser asterisk enemy never fired", _currentSeed);

        // WHEN
        Wound(enemy, ENEMY_HEALTH - woundedRetreat.GetWoundedHealth());
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "Wounding hit did not start the retreat", _currentSeed);
        AssertWrapper.AreEqual(0, TestUtils.GetEnemyLaserBeams().Count, "Wounded laser asterisk enemy kept firing", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShotWoundsAndStartsRetreat()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var enemy = LoadPlainEnemy();
        var woundedRetreat = TestUtils.GetWoundedRetreat(enemy);
        yield return null;

        var woundedHealth = woundedRetreat.GetWoundedHealth();
        TestUtils.SetEnemiesHealth(woundedHealth + TestUtils.DEFAULT_POWER_1);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(woundedHealth, TestUtils.GetEnemyHealth(), "Ball Shot did not hurt the enemy exactly once", _currentSeed);
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "Ball Shot wound did not start the retreat", _currentSeed);
    }

    // The plain enemy from the scene's ObjectLoader (a Lame Enemy, its movement stopped), placed to the right of the Halalit.
    private GameObject LoadPlainEnemy()
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        return enemy;
    }

    private void Wound(GameObject enemy, int harm)
    {
        enemy.GetComponent<Health>().TryChangeHealth(-harm);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
