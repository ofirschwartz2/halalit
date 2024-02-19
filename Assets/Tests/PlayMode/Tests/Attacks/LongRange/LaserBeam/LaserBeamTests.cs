using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class LaserBeamTests
{

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_TARGET_NAME = "TestingWithTarget";

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

        TestUtils.SetUpShot(AttackName.LASER_BEAM);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Assert.IsNotNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed(1683092413);
        
        float shootingTime = 3f;
        TestUtils.SetUpShot(AttackName.LASER_BEAM);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        float time = 0;
        while (time < shootingTime) 
        {
            weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
            yield return null;

            time += Time.deltaTime;
        }

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        var startOfLaser = shot.GetComponent<LineRenderer>().GetPosition(0);

        // THEN
        Assert.IsNotNull(shot, $"Seed: {seed}");
        Assert.AreEqual(startOfLaser.x, weaponAttack.transform.position.x, $"Seed: {seed}");
        Assert.AreEqual(startOfLaser.y, weaponAttack.transform.position.y, $"Seed: {seed}");

        // GIVEN
        Vector2 endOfLaser;

        // WHEN

        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        yield return null;

        do
        {
            endOfLaser = shot.GetComponent<LineRenderer>().GetPosition(1);
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        Assert.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(endOfLaser), $"Seed: {seed}");

    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.LASER_BEAM);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.5f;

        TestUtils.SetRandomTargetPosition();
        yield return null;
        var targetNearestPosition = TestUtils.GetTargetNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetNearestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Assert.IsNotNull(shot, $"Seed: {seed}");

        // GIVEN
        Vector2 lastShotPosition, newLastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(touchOnJoystick);
            yield return null;

            newLastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
        } while (newLastShotPosition != lastShotPosition);

        // THEN
        Assert.AreEqual(lastShotPosition.x, targetNearestPosition.x, acceptedDelta, $"Seed: {seed}");
        Assert.AreEqual(lastShotPosition.y, targetNearestPosition.y, acceptedDelta, $"Seed: {seed}");

    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        var acceptedDelta = 0.01f;
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        TestUtils.SetUpShot(AttackName.LASER_BEAM);
        yield return null;


        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Assert.IsNotNull(shot, $"Seed: {seed}");

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
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