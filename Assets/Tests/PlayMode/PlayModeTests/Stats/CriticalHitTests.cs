using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

class CriticalHitTests
{
    private int _currentSeed;
    private WeaponAttack _weaponAttack;
    private WeaponMovement _weaponMovement;
    private float _originalTargetHealth;
    private Vector2 _attackJoystickTouch;

    private static readonly AttackStats[] _statsValuesAttackWithSuccessfulCriticalHitTest = new AttackStats[] { TestUtils.DEFAULT_ATTACK_STATS_3, TestUtils.DEFAULT_ATTACK_STATS_4 };
    private static readonly AttackStats[] _statsValuesAttackWithUnsuccessfulCriticalHitTest = new AttackStats[] { TestUtils.DEFAULT_ATTACK_STATS_5, TestUtils.DEFAULT_ATTACK_STATS_6 };

    private void LoadAttackTestData()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        _weaponAttack = TestUtils.GetWeaponAttack();
        _weaponMovement = TestUtils.GetWeaponMovement();
        _originalTargetHealth = TestUtils.GetEnemyHealth();

        Vector2 targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        _attackJoystickTouch = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, _weaponAttack.GetAttackJoystickEdge());
    }

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator AttackWithSuccessfulCriticalHitTest([ValueSource(nameof(_statsValuesAttackWithSuccessfulCriticalHitTest))] AttackStats attackStatsAttackWithSuccessfulCriticalHitTest)
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        LoadAttackTestData();

        // SeedlessRandomGenerator.SetTestingExpectedValue(attackStats.Luck - 1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStatsAttackWithSuccessfulCriticalHitTest);
        yield return null;

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
        float expectedDamage = (float) Math.Floor(attackStatsAttackWithSuccessfulCriticalHitTest.Power * attackStatsAttackWithSuccessfulCriticalHitTest.CriticalHit);
        AssertWrapper.AreEqual(_originalTargetHealth - expectedDamage, newTargetHealth, "Target Health Didn't drop correctly", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AttackWithUnsuccessfulCriticalHitTest([ValueSource(nameof(_statsValuesAttackWithUnsuccessfulCriticalHitTest))] AttackStats attackStatsAttackWithUnsuccessfulCriticalHitTest)
    {
        // GIVEN
        // SeedlessRandomGenerator.SetTestingExpectedValue(attackStats.Luck + 1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStatsAttackWithUnsuccessfulCriticalHitTest);
        yield return null;

        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
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
        AssertWrapper.AreEqual(_originalTargetHealth - attackStatsAttackWithUnsuccessfulCriticalHitTest.Power, newTargetHealth, "Target Health Didn't drop correctly", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}
