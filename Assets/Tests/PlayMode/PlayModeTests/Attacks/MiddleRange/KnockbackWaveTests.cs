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
    private int _currentSeed;
    private const Tag ATTACK_TAG = Tag.KNOCKBACK_WAVE;
    private const AttackName ATTACK_NAME = AttackName.KNOCKBACK_WAVE;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN     
        yield return null;
        TestUtils.SetTestMode();   
        TestUtils.SetUpShot(ATTACK_NAME);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.GreaterOrEqual(thisKnockbackLifeTime, supposedKnockbackLifeTime, "Ended Under KnockbackLifeTime", _currentSeed);

        AssertWrapper.AreEqual(lastShotPosition.x, supposedLastKnockbackPosition.x, "Finish Position Unexpected", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, supposedLastKnockbackPosition.y, "Finish Position Unexpected", _currentSeed, acceptedDelta);
    }

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetClosestPositionBeforeHit = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPositionBeforeHit, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());

        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;
        Vector2 lastTargetPosition, thisTargetPosition;
        lastTargetPosition = TestUtils.GetEnemyPosition();
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

            thisTargetPosition = TestUtils.GetEnemyPosition();

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
        AssertWrapper.GreaterOrEqual(Math.Abs(lastShotPosition.x), Math.Abs(targetClosestPositionBeforeHit.x), "Did not pass through target", _currentSeed);
        AssertWrapper.GreaterOrEqual(Math.Abs(lastShotPosition.y), Math.Abs(targetClosestPositionBeforeHit.y), "Did not pass through target", _currentSeed);

        sameDirection = Utils.GetDirectionVector(lastTargetPosition, thisTargetPosition);
        AssertWrapper.AreEqual(firstHiDirection.x, sameDirection.x, ">1 Hits", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(firstHiDirection.y, sameDirection.y, ">1 Hits", _currentSeed, acceptedDelta);

        var targetClosestPositionAfterHit = TestUtils.GetEnemyNearestPositionToHalalit();
        if (targetClosestPositionBeforeHit.x != 0) 
        {
            AssertWrapper.Greater(Math.Abs(targetClosestPositionAfterHit.x), Math.Abs(targetClosestPositionBeforeHit.x), "Did not Knockback", _currentSeed);
        }
        if (targetClosestPositionBeforeHit.y != 0)
        {
            AssertWrapper.Greater(Math.Abs(targetClosestPositionAfterHit.y), Math.Abs(targetClosestPositionBeforeHit.y), "Did not Knockback", _currentSeed);
        }

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator KnockbackingVector()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;
        TestUtils.SetRandomEnemyPosition();
        yield return null;
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(TestUtils.GetEnemyPosition(), weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        Vector2 
            newTargetPosition = TestUtils.GetEnemyPosition(), 
            oldTargetPosition,
            shotDirection,
            shotPosition;

        // WHEN
        do
        {
            oldTargetPosition = newTargetPosition;
            shotDirection = shot.GetComponent<Rigidbody2D>().velocity.normalized;
            shotPosition = shot.transform.position;
            yield return null;

            newTargetPosition = TestUtils.GetEnemyPosition();
        } while (Utils.Vector2ToDegrees(newTargetPosition) == Utils.Vector2ToDegrees(oldTargetPosition));

        // THEN
        var targetToShotDirection = (oldTargetPosition - shotPosition).normalized;
        var combinedVector = (shotDirection + targetToShotDirection).normalized;
        AssertWrapper.AreEqual(Utils.Vector2ToDegrees(combinedVector), Utils.Vector2ToDegrees(TestUtils.GetEnemyMovementDirection()), "Knockback Direction not as expected", _currentSeed, 0.1f);
    }

    [UnityTest]
    public IEnumerator DoubleHitCheck()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(TestUtils.GetEnemyPosition(), weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        var targetOriginalHealth = TestUtils.GetEnemyHealth();
        float targetHealthAfterHit = targetOriginalHealth;
        bool didGetHit = false;

        // WHEN
        do
        {
            if (!didGetHit) 
            {
                targetHealthAfterHit = TestUtils.GetEnemyHealth();
                if (targetHealthAfterHit != targetOriginalHealth) 
                {
                    didGetHit = true;
                }
            }
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.Greater(targetOriginalHealth, targetHealthAfterHit, "Did Not Hit Target", _currentSeed);
        AssertWrapper.AreEqual(targetHealthAfterHit, TestUtils.GetEnemyHealth(), ">1 Hits", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        TestUtils.SetUpShot(ATTACK_NAME);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.IsFalse(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't finish In The Edges", _currentSeed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Directions difference",
            _currentSeed,
            acceptedDelta);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}