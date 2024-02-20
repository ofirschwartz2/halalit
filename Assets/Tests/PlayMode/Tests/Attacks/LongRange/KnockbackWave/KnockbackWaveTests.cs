using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class KnockbackWaveTests
{

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_TARGET_NAME = "TestingWithTarget";
    private const AttackName SHOT_NAME = AttackName.KNOCKBACK_WAVE;
    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_TARGET_NAME);
                break;
            default:
                SceneManager.LoadScene(SCENE_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        Assert.IsNotNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        
        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        Assert.IsNotNull(shot, $"Seed: {seed}");

        // GIVEN
        var thisKnockbackLifeTime = 0f;
        var supposedKnockbackLifeTime = shot.GetComponent<KnockbackWave>().GetLifetime();
        var supposedKnockbackSpeed = shot.GetComponent<KnockbackWave>().GetSpeed();
        var supposedLastKnockbackPosition = shot.transform.up * supposedKnockbackSpeed * supposedKnockbackLifeTime;
        var acceptedDelta = 1f;
        Vector2 lastShotPosition;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            thisKnockbackLifeTime += Time.deltaTime;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);

        thisKnockbackLifeTime += Time.deltaTime;

        // THEN
        Assert.GreaterOrEqual(thisKnockbackLifeTime, supposedKnockbackLifeTime, $"Seed: {seed}");
        Assert.AreEqual(lastShotPosition.x, supposedLastKnockbackPosition.x, acceptedDelta, $"Seed: {seed}");
        Assert.AreEqual(lastShotPosition.y, supposedLastKnockbackPosition.y, acceptedDelta, $"Seed: {seed}");
    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomTargetPosition();
        yield return null;
        var targetClosestPositionBeforeHit = TestUtils.GetTargetNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPositionBeforeHit, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        Assert.IsNotNull(shot, $"Seed: {seed}");

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);

        // THEN
        Assert.GreaterOrEqual(Math.Abs(lastShotPosition.x), Math.Abs(targetClosestPositionBeforeHit.x), $"Seed: {seed}");
        Assert.GreaterOrEqual(Math.Abs(lastShotPosition.y), Math.Abs(targetClosestPositionBeforeHit.y), $"Seed: {seed}");

        var targetClosestPositionAfterHit = TestUtils.GetTargetNearestPositionToHalalit();
        Assert.Greater(Math.Abs(targetClosestPositionAfterHit.x), Math.Abs(targetClosestPositionBeforeHit.x), $"Seed: {seed}");
        Assert.Greater(Math.Abs(targetClosestPositionAfterHit.y), Math.Abs(targetClosestPositionBeforeHit.y), $"Seed: {seed}");

    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.01f;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        Assert.IsNotNull(shot, $"Seed: {seed}");

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);

        // THEN
        Assert.IsFalse(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), $"Seed: {seed}");

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);
        
        Assert.AreEqual(
            weaponDirection,
            shotDirection,
            acceptedDelta,
            $"Seed: {seed}");

    }

}