using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BallShotTests
{
    private int _currentSeed;

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
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", _currentSeed);

    }

    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());
        var originalTargetHealth = TestUtils.GetEnemyHealth();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Didn't Finish On Edges", _currentSeed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var shotDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            shotDirection,
            "Weapon vs Shot Direction",
            _currentSeed,
            acceptedDelta);

    }


    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}