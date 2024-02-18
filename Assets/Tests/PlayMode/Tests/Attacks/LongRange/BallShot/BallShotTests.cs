using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BallShotTests
{

    private const string SCENE_NAME = "Playground";
    private const string SCENE_WITH_TARGET_NAME = "PlaygroundWithTarget";

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
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpShot(AttackName.BALL_SHOT);

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        
        Random.InitState(seed);

        TestUtils.SetUpShot(AttackName.BALL_SHOT);

        yield return null;

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"Seed: {seed}");

        Vector2 lastShotPosition;

        do {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        Assert.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), $"Seed: {seed}");

    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpShot(AttackName.BALL_SHOT);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        TestUtils.SetRandomTargetPosition();

        yield return null;

        var targetClosestPosition = TestUtils.GetTargetNearestPositionToHalalit();

        var acceptedDelta = 0.3f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);

        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"Seed: {seed}");

        Vector2 lastShotPosition;

        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        Assert.AreEqual(lastShotPosition.x, targetClosestPosition.x, acceptedDelta, $"Seed: {seed}");
        Assert.AreEqual(lastShotPosition.y, targetClosestPosition.y, acceptedDelta, $"Seed: {seed}");

    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        var acceptedDelta = 0.01f;
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpShot(AttackName.BALL_SHOT);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        yield return null;

        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);

        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"Seed: {seed}");

        Vector2 lastShotPosition;

        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        Assert.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), $"Seed: {seed}");

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);
        
        Assert.AreEqual(
            weaponDirection,
            shotDirection,
            acceptedDelta,
            $"Seed: {seed}");

    }

}