using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class GrappleShotTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.GRAPPLE_SHOT;
    private const float TARGET_DISTANCE = 5f;
    private const float WAIT_AFTER_SHOT = 0.5f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator PullsHalalitToAsteroid()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(100);
        var halalit = TestUtils.GetHalalit();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        Vector2 halalitStartPosition = halalit.transform.position;
        var pullDirection = touchOnJoystick.normalized;
        TestUtils.SetAsteroidPosition(halalitStartPosition + pullDirection * TARGET_DISTANCE);
        yield return null;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        var furthestProgress = 0f;

        // WHEN
        do
        {
            furthestProgress = Mathf.Max(furthestProgress, Vector2.Dot((Vector2)halalit.transform.position - halalitStartPosition, pullDirection));
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        var endOfWait = Time.time + WAIT_AFTER_SHOT;
        while (Time.time < endOfWait)
        {
            furthestProgress = Mathf.Max(furthestProgress, Vector2.Dot((Vector2)halalit.transform.position - halalitStartPosition, pullDirection));
            yield return null;
        }

        // THEN
        AssertWrapper.Greater(furthestProgress, 1f, "Halalit was not pulled to the asteroid", _currentSeed);
        AssertWrapper.AreEqual(0, TestUtils.GetAsteroidMovement().GetSpeed(), "Asteroid was moved", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PullsEnemyToHalalit()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(100);
        var halalit = TestUtils.GetHalalit();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        Vector2 halalitStartPosition = halalit.transform.position;
        TestUtils.SetEnemyPosition(halalitStartPosition + touchOnJoystick.normalized * TARGET_DISTANCE);
        yield return null;

        var startDistance = Vector2.Distance(TestUtils.GetEnemyPosition(), halalit.transform.position);

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        var closestDistance = startDistance;

        // WHEN
        do
        {
            closestDistance = Mathf.Min(closestDistance, Vector2.Distance(TestUtils.GetEnemyPosition(), halalit.transform.position));
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        var endOfWait = Time.time + WAIT_AFTER_SHOT;
        while (Time.time < endOfWait)
        {
            closestDistance = Mathf.Min(closestDistance, Vector2.Distance(TestUtils.GetEnemyPosition(), halalit.transform.position));
            yield return null;
        }

        // THEN
        AssertWrapper.Greater(startDistance - closestDistance, 1f, "Enemy was not pulled to the Halalit", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NoPullWithoutTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var halalit = TestUtils.GetHalalit();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        Vector2 halalitStartPosition = halalit.transform.position;
        var acceptedDelta = 0.01f;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(halalitStartPosition, halalit.transform.position, "Halalit moved without a target", _currentSeed, acceptedDelta);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
