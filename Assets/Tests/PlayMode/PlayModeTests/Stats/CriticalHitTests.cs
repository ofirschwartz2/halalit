﻿using Assets.Enums;
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

    private static readonly AttackStats[] _statsValues = new AttackStats[] { TestUtils.DEFAULT_ATTACK_STATS_3, TestUtils.DEFAULT_ATTACK_STATS_4 };

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
        SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_ENEMY_NAME);
        SeedlessRandomGenerator.SetUseTestingExpectedValue(true);
    }

    [UnityTest]
    public IEnumerator AttackWithSuccessfulCriticalHitTest([ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        SeedlessRandomGenerator.SetTestingExpectedValue(attackStats.Luck - 1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStats);
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
        float expectedDamage = (float) Math.Floor(attackStats.Power * attackStats.CriticalHit);
        AssertWrapper.AreEqual(_originalTargetHealth - expectedDamage, newTargetHealth, "Target Health Didn't drop correctly", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AttackWithUnsuccessfulCriticalHitTest([ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        SeedlessRandomGenerator.SetTestingExpectedValue(attackStats.Luck + 1);
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStats);
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

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}
