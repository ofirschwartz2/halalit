using Assets.Enums;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class RateTests
{
    private int _currentSeed;
    private WeaponAttack _weaponAttack;
    private WeaponMovement _weaponMovement;
    private Vector2 _attackJoystickTouch;
    private float _acceptedDelta;

    private static readonly AttackStats[] _statsValues = new AttackStats[] { TestUtils.DEFAULT_ATTACK_STATS_1, TestUtils.DEFAULT_ATTACK_STATS_2 };

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        if (testName.Contains(AttackShotType.DESCRETE.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            SceneManager.LoadScene(TestUtils.TEST_SCENE_WITHOUT_TARGET_NAME);
        }
        else {
            SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_ENEMY_NAME);
        }
    }

    private void LoadAttackTestData()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        _weaponAttack = TestUtils.GetWeaponAttack();
        _weaponMovement = TestUtils.GetWeaponMovement();
        _acceptedDelta = 0.03f;

        string testName = TestContext.CurrentContext.Test.MethodName;
        if (testName.Contains(AttackShotType.DESCRETE.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            _attackJoystickTouch = TestUtils.GetRandomTouchOverAttackTrigger(_weaponAttack.GetAttackJoystickEdge());
        }
        else
        {
            Vector2 targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
            _attackJoystickTouch = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, _weaponAttack.GetAttackJoystickEdge());
        }
    }

    [UnityTest]
    public IEnumerator AttackWithDescreteWeaponTest([ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.BALL_SHOT, attackStats);
        yield return null;

        LoadAttackTestData();

        // WHEN
        _weaponMovement.TryChangeWeaponPosition(_attackJoystickTouch);
        yield return null;

        IEnumerator coroutine = TestUtils.GetTimeBetweenProjectedDescreteShot(2, _weaponAttack, _attackJoystickTouch);
        while (coroutine.MoveNext())
        {
            yield return null;
        }
        float timeBetweenShots = (float)coroutine.Current;

        // THEN
        AssertWrapper.AreEqual(attackStats.Rate, timeBetweenShots, "The time between the shots wasn't as the attack rate", _currentSeed, _acceptedDelta);
    }

    [UnityTest]
    public IEnumerator AttackWithConsecutiveWeaponTest([ValueSource(nameof(_statsValues))] AttackStats attackStats)
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.LASER_BEAM, attackStats);
        yield return null;

        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        LoadAttackTestData();

        // WHEN
        _weaponMovement.TryChangeWeaponPosition(_attackJoystickTouch);
        yield return null;

        _weaponAttack.HumbleFixedUpdate(_attackJoystickTouch);
        yield return null;

        IEnumerator coroutine = TestUtils.GetTimeBetweenHittingConsecutiveShot(3, _weaponAttack, _attackJoystickTouch);
        while (coroutine.MoveNext())
        {
            yield return null;
        }
        float timeBetweenShotHits = (float)coroutine.Current;

        // THEN
        AssertWrapper.AreEqual(attackStats.Rate, timeBetweenShotHits, "The time between the shot hits wasn't as the attack rate", _currentSeed, _acceptedDelta);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}