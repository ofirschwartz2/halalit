using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class LaserBeamTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.LASER_BEAM;

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
        TestUtils.SetUpShot(ATTACK_NAME);
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
        float shootingTime = 3f;
        TestUtils.SetUpShot(ATTACK_NAME);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);
        AssertWrapper.AreEqual(startOfLaser.x, weaponAttack.transform.position.x, "Laser Not Starting from Weapon", _currentSeed);
        AssertWrapper.AreEqual(startOfLaser.y, weaponAttack.transform.position.y, "Laser Not Starting from Weapon", _currentSeed);

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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(endOfLaser), "Laser Not Ending on Edges", _currentSeed);

    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.5f;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland();
        yield return null;

        TestUtils.SetRandomEnemyPosition();
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return new WaitForSeconds(1);

        var targetNearestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetNearestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;
        
        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.AreEqual(lastShotPosition.x, targetNearestPosition.x, "Laser Is Not Touching Target", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetNearestPosition.y, "Laser Is Not Touching Target", _currentSeed, acceptedDelta);
        yield return null;
        yield return null;

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var acceptedDelta = 0.01f;
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        TestUtils.SetUpShot(ATTACK_NAME);
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
        AssertWrapper.IsNotNull(shot, _currentSeed);

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
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Laser Not Ending on Edges", _currentSeed);

        var weaponDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation));
        var laserDirection = Utils.Vector2ToDegrees(lastShotPosition);

        AssertWrapper.AreEqual(
            weaponDirection,
            laserDirection,
            "Weapon vs Laser Direction",
            _currentSeed,
            acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}