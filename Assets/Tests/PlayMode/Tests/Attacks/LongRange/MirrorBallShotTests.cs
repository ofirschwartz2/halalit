using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MirrorBallShotTests
{
    private const string FUNCTION_AMOUNT_OF_BOUNCES_CHECK_NAME = "AmountOfBouncesCheck";
    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    private const string FUNCTION_BOUNCE_DIRECTION_CHECK_NAME = "BounceDirectionCheck";

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
            case FUNCTION_BOUNCE_DIRECTION_CHECK_NAME:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_TARGET_NAME);
                break;
            case FUNCTION_AMOUNT_OF_BOUNCES_CHECK_NAME:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_FOR_BOUNCES);
                break;
            default:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITHOUT_TARGET_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        
        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", seed);

    }

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomTargetPosition();
        var originalTargetHealth = TestUtils.GetTargetHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetTargetNearestPositionToHalalit();
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
        Vector2 lastShotPosition = Vector2.zero, thisShotPosition = Vector2.zero;
        Vector2 startShotDirection = Vector2.zero, endShotDirection = Vector2.zero;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            if (shot != null) 
            {
                thisShotPosition = shot.transform.position;
            }
            if (thisShotPosition != lastShotPosition && startShotDirection == Vector2.zero) 
            {
                startShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
            }
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        endShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
        AssertWrapper.AreNotEqual(startShotDirection.x, endShotDirection.x, "Didn't Change Direction", seed);
        AssertWrapper.AreNotEqual(startShotDirection.y, endShotDirection.y, "Didn't Change Direction", seed);
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", seed);
        
        var newTargetHealth = TestUtils.GetTargetHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }
  
    [UnityTest]
    public IEnumerator BounceDirectionCheck()
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.RotateTarget(90);
        yield return null;

        var touchOnJoystick = new Vector2(1f, 0.2f);

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        Vector2 lastShotPosition;
        Vector2 endShotDirection;
        Vector2 thisShotPosition = Vector2.zero;
        Vector2 startShotDirection = Vector2.zero;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            if (shot != null)
            {
                thisShotPosition = shot.transform.position;
            }
            if (thisShotPosition != lastShotPosition && startShotDirection == Vector2.zero)
            {
                startShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
            }
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        endShotDirection = Utils.GetDirectionVector(lastShotPosition, thisShotPosition);
        AssertWrapper.AreNotEqual(startShotDirection.x, endShotDirection.x, "Didn't Change Direction");
        AssertWrapper.AreNotEqual(startShotDirection.y, endShotDirection.y, "Didn't Change Direction");
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges");
    }

    [UnityTest]
    public IEnumerator AmountOfBouncesCheck()
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var originalTargetsHealth = TestUtils.GetAllTargetsHealth().First();
        int actualNumberOfHits = 0;
        var touchOnJoystick = Vector2.right;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot);

        // GIVEN
        var supposedNumberOfHits = shot.GetComponent<MirrorBallShot>().GetMaxBounces() + 1;

        // WHEN
        do
        {
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        var newTargetsHealth = TestUtils.GetAllTargetsHealth();

        foreach (var newTargetHealth in newTargetsHealth) 
        {
            if (newTargetHealth < originalTargetsHealth) 
            {
                actualNumberOfHits++;
            }
        }

        AssertWrapper.AreEqual(supposedNumberOfHits, actualNumberOfHits, "Wrong Number of Hits");
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.MIRROR_BALL_SHOT);
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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", seed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Direction",
            seed,
            acceptedDelta);
    }
}