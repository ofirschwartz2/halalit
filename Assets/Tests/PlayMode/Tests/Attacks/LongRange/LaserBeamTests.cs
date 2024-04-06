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
    private const string SCENE_WITH_NEMY_NAME = "TestingWithEnemy";
    private const AttackName SHOT_NAME = AttackName.LASER_BEAM;

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_NEMY_NAME);
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
        var seed = TestUtils.SetRandomSeed(1683092413);
        
        float shootingTime = 3f;
        TestUtils.SetUpShot(SHOT_NAME);
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
        AssertWrapper.IsNotNull(shot, seed);
        AssertWrapper.AreEqual(startOfLaser.x, weaponAttack.transform.position.x, "Laser Not Starting from Weapon", seed);
        AssertWrapper.AreEqual(startOfLaser.y, weaponAttack.transform.position.y, "Laser Not Starting from Weapon", seed);

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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(endOfLaser), "Laser Not Ending on Edges", seed);

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
        var acceptedDelta = 0.5f;

        TestUtils.SetRandomEnemyPosition();
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return new WaitForSeconds(1);

        var targetNearestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetNearestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.AreEqual(lastShotPosition.x, targetNearestPosition.x, "Laser Is Not Touching Target", seed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetNearestPosition.y, "Laser Is Not Touching Target", seed, acceptedDelta);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
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

        TestUtils.SetUpShot(SHOT_NAME);
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
        AssertWrapper.IsNotNull(shot, seed);

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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Laser Not Ending on Edges", seed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var laserDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            laserDirection,
            "Weapon vs Laser Direction",
            seed,
            acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}