using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SwapShotTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.SWAP_SHOT;
    private const float TARGET_DISTANCE = 5f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator SwapsHalalitAndEnemy()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<SwapShot>(ATTACK_NAME, _currentSeed);
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

        var enemyStartPosition = TestUtils.GetEnemyPosition();
        var acceptedDelta = 0.5f;

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

        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(enemyStartPosition, halalit.transform.position, "Halalit was not moved to the enemy's place", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(halalitStartPosition, TestUtils.GetEnemyPosition(), "Enemy was not moved to the Halalit's place", _currentSeed, acceptedDelta);
    }

    [UnityTest]
    public IEnumerator AsteroidOnlyEndsShot()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<SwapShot>(ATTACK_NAME, _currentSeed);
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
        TestUtils.SetAsteroidPosition(halalitStartPosition + touchOnJoystick.normalized * TARGET_DISTANCE);
        yield return null;

        var acceptedDelta = 0.01f;

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        var shotLifetime = shot.GetComponent<BallShot>().GetLifeTime();
        var shotStartTime = Time.time;

        // WHEN
        do
        {
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        var shotTimeAlive = Time.time - shotStartTime;
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsTrue(shotLifetime <= 0 || shotTimeAlive < shotLifetime, "Shot was not ended by the asteroid", _currentSeed);
        AssertWrapper.AreEqual(halalitStartPosition, halalit.transform.position, "Halalit moved after hitting an asteroid", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(0, TestUtils.GetAsteroidMovement().GetSpeed(), "Asteroid was moved", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
