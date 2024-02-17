using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BallShotTests
{

    private const string SCENE_NAME = "Playground";
    private const string SCENE_WITH_TARGET_NAME = "PlaygroundWithTarget";

    private const string FUNCTION_BALL_SHOT_SHOOTING_WITH_TARGET_NAME = "BallShotShootingWithTarget";

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_BALL_SHOT_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_TARGET_NAME);
                break;
            default:
                SceneManager.LoadScene(SCENE_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator BallShotShooting()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpBallShot();

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator BallShotShootingWithoutTarget()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpBallShot();

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot);

        Vector2 lastShotPosition;

        do {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        Assert.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), $"Seed: {seed}");

    }

    [UnityTest]
    public IEnumerator BallShotShootingWithTarget()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpBallShot();

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

    [UnitTest]
    public IEnumerator ShootingInDirection()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        TestUtils.SetUpBallShot();

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

        Assert.AreEqual(lastShotPosition.x, targetPosition.x, 0.3f, $"Seed: {seed}");
        Assert.AreEqual(lastShotPosition.y, targetPosition.y, 0.3f, $"Seed: {seed}");

    }

}