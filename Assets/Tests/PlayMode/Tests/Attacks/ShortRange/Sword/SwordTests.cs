using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class SwordTests
{

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    private const AttackName SHOT_NAME = AttackName.SWORD;

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SLASHING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_ENEMY_NAME);
                break;
            default:
                SceneManager.LoadScene(SCENE_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Slashing()
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
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);
    }

    private const string FUNCTION_SLASHING_WITH_TARGET_NAME = "SlashingWithTarget";
    [UnityTest]
    public IEnumerator SlashingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(1.5f);
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 1f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN

        //TODO: Check target's position
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
        AssertWrapper.IsTrue(true, "");
    }

    [UnityTest]
    public IEnumerator SlashLifetime()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        float supposedLifetime = shot.GetComponent<Sword>().GetAttacktime();
        float actualShotTime = 0;

        // WHEN
        do
        {
            actualShotTime += Time.deltaTime;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(supposedLifetime, actualShotTime, "Sword Lifetime Not As Expected", seed, acceptedDelta);

    }

    [UnityTest]
    public IEnumerator SlashRotationRange()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;
            
        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        var swordRotationRange = shot.GetComponent<Sword>().GetSwordRotationRange();
        var supposedFromRotation = Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, -0.5f * swordRotationRange));
        var actualFromRotation = Utils.GetRotationAsVector2(shot.transform.rotation);
        var supposedToRotation = Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, 0.5f * swordRotationRange));
        Vector2 actualToRotation;

        // WHEN
        do
        {
            actualToRotation = Utils.GetRotationAsVector2(shot.transform.rotation);
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(supposedFromRotation.x, actualFromRotation.x, "Sword From Rotation Not As Expected", seed, acceptedDelta);
        AssertWrapper.AreEqual(supposedFromRotation.y, actualFromRotation.y, "Sword From Rotation Not As Expected", seed, acceptedDelta);
        AssertWrapper.AreEqual(supposedToRotation.x, actualToRotation.x, "Sword To Rotation Not As Expected", seed, acceptedDelta);
        AssertWrapper.AreEqual(supposedToRotation.y, actualToRotation.y, "Sword To Rotation Not As Expected", seed, acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}