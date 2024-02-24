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
            case FUNCTION_KNOCKBACKINT_VECTOR_NAME:
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
        AssertWrapper.IsNotNull(shot, seed);
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
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.GreaterOrEqual(thisKnockbackLifeTime, supposedKnockbackLifeTime, "Ended Under KnockbackLifeTime", seed);

        AssertWrapper.AreEqual(lastShotPosition.x, supposedLastKnockbackPosition.x, "Finish Position Unexpected", seed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, supposedLastKnockbackPosition.y, "Finish Position Unexpected", seed, acceptedDelta);
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
        var originalTargetHealth = TestUtils.GetTargetHealth();
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

        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;
        Vector2 lastTargetPosition, thisTargetPosition;
        lastTargetPosition = TestUtils.GetTargetPosition();
        thisTargetPosition = lastTargetPosition;
        Vector2 firstHiDirection = Vector2.zero, sameDirection = Vector2.zero;
        bool gotHit = false;
        float acceptedDelta = 0.1f;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            lastTargetPosition = thisTargetPosition;
            yield return null;

            thisTargetPosition = TestUtils.GetTargetPosition();

            if (!gotHit)
            {
                firstHiDirection = Utils.GetDirectionVector(lastTargetPosition, thisTargetPosition);
                if (firstHiDirection != Vector2.zero) 
                {
                    gotHit = true;
                }
            }

            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);


        // THEN
        AssertWrapper.GreaterOrEqual(Math.Abs(lastShotPosition.x), Math.Abs(targetClosestPositionBeforeHit.x), "Did not pass through target", seed);
        AssertWrapper.GreaterOrEqual(Math.Abs(lastShotPosition.y), Math.Abs(targetClosestPositionBeforeHit.y), "Did not pass through target", seed);

        sameDirection = Utils.GetDirectionVector(lastTargetPosition, thisTargetPosition);
        AssertWrapper.AreEqual(firstHiDirection.x, sameDirection.x, ">1 Hits", seed, acceptedDelta);
        AssertWrapper.AreEqual(firstHiDirection.y, sameDirection.y, ">1 Hits", seed, acceptedDelta);

        var targetClosestPositionAfterHit = TestUtils.GetTargetNearestPositionToHalalit();
        if (targetClosestPositionBeforeHit.x != 0) 
        {
            AssertWrapper.Greater(Math.Abs(targetClosestPositionAfterHit.x), Math.Abs(targetClosestPositionBeforeHit.x), "Did not Knockback", seed);
        }
        if (targetClosestPositionBeforeHit.y != 0)
        {
            AssertWrapper.Greater(Math.Abs(targetClosestPositionAfterHit.y), Math.Abs(targetClosestPositionBeforeHit.y), "Did not Knockback", seed);
        }

        var newTargetHealth = TestUtils.GetTargetHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }

    private const string FUNCTION_KNOCKBACKINT_VECTOR_NAME = "KnockbackingVector";
    [UnityTest]
    public IEnumerator KnockbackingVector()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();


        var targetClosestPositionBeforeHit = TestUtils.GetTargetNearestPositionToHalalit();
        var touchOnJoystick = new Vector2(1,0);

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // WHEN
        do
        {
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.Fail("Amir");
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
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.IsFalse(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't finish In The Edges", seed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Directions difference",
            seed,
            acceptedDelta);

    }

}