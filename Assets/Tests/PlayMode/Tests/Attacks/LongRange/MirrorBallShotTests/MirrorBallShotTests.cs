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

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    private const string SCENE_FOR_BOUNCES_NAME = "TestingForBounces";
    private const AttackName SHOT_NAME = AttackName.MIRROR_BALL_SHOT;

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
            case FUNCTION_BOUNCE_DIRECTION_CHECK_NAME:
                SceneManager.LoadScene(SCENE_WITH_ENEMY_NAME);
                break;
            case FUNCTION_AMOUNT_OF_BOUNCES_CHECK_NAME:
                SceneManager.LoadScene(SCENE_FOR_BOUNCES_NAME);
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
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
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

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 0.5f;
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
        
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }

    private const string FUNCTION_BOUNCE_DIRECTION_CHECK_NAME = "BounceDirectionCheck";
    [UnityTest]
    public IEnumerator BounceDirectionCheck()
    {
        // GIVEN

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.RotaeEnemy(90);
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
        AssertWrapper.AreNotEqual(startShotDirection.x, endShotDirection.x, "Didn't Change Direction");
        AssertWrapper.AreNotEqual(startShotDirection.y, endShotDirection.y, "Didn't Change Direction");
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges");
        AssertWrapper.Fail("TODO: fix bounce");
    }

    private const string FUNCTION_AMOUNT_OF_BOUNCES_CHECK_NAME = "AmountOfBouncesCheck";
    [UnityTest]
    public IEnumerator AmountOfBouncesCheck()
    {
        // GIVEN
        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var originalTargetsHealth = TestUtils.GetAllEnemiesHealth().First();
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
        var newTargetsHealth = TestUtils.GetAllEnemiesHealth();

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