using Assets.Enums;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PowerTests
{
    private int _currentSeed;
    private WeaponAttack _weaponAttack;
    private WeaponMovement _weaponMovement;
    private float _originalTargetHealth;
    private Vector2 _attackJoystickTouch;

    private static readonly AttackStats[] _statsValues = new AttackStats[] { TestUtils.DEFAULT_ATTACK_STATS_1, TestUtils.DEFAULT_ATTACK_STATS_2 };
    private static readonly int[] _consecutiveHitsCountsValues = new int[] { 1, 3 };

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_ENEMY_NAME);
    }

    private void LoadAttackTestData()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        _weaponAttack = TestUtils.GetWeaponAttack();
        _weaponMovement = TestUtils.GetWeaponMovement();
        _originalTargetHealth = TestUtils.GetEnemyHealth();

        Vector2 targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        _attackJoystickTouch = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, _weaponAttack.GetAttackJoystickEdge());
    }

    [UnityTest]
    public IEnumerator AttackWithDescreteWeaponTest([ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStats);
        TestUtils.SetEnemiesSeededNumbers();
        yield return null;

        TestUtils.SetTargetPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        LoadAttackTestData();

        yield return new WaitForSeconds(1f);

        // WHEN
        _weaponMovement.TryChangeWeaponPosition(_attackJoystickTouch);
        yield return null;

        _weaponAttack.HumbleFixedUpdate(_attackJoystickTouch);
        yield return null;

        IEnumerator coroutine = TestUtils.GetDescreteShotPositionHittingTarget();


        do
        {
            yield return null;
        } while (coroutine.MoveNext());

        yield return new WaitForSeconds(1f);

        // THEN
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.AreEqual(_originalTargetHealth - attackStats.Power, newTargetHealth, "Target Health Didn't drop correctly", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AttackWithConsecutiveWeaponTest([ValueSource(nameof(_consecutiveHitsCountsValues))] int consecutiveHitsCount, [ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.LASER_BEAM, attackStats);
        TestUtils.SetEnemiesSeededNumbers();
        yield return null;

        TestUtils.SetTargetPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        LoadAttackTestData();

        yield return new WaitForSeconds(1f);

        // WHEN
        _weaponMovement.TryChangeWeaponPosition(_attackJoystickTouch);
        yield return null;

        _weaponAttack.HumbleFixedUpdate(_attackJoystickTouch);
        yield return null;

        IEnumerator coroutine = TestUtils.GetConsecutiveShotPositionHitingTarget(consecutiveHitsCount, attackStats.Rate, _weaponAttack, _attackJoystickTouch);

        while (coroutine.MoveNext())
        {
            yield return null;
        }


        // THEN
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.AreEqual(_originalTargetHealth - attackStats.Power * consecutiveHitsCount, newTargetHealth, "Target Health Didn't drop correctly", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}