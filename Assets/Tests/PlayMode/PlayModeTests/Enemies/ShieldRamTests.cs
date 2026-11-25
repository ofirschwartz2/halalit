using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ShieldRamTests
{
    private const int ENEMY_HEALTH = 100;
    private const float SHOT_HIT_DELTA = 0.5f;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator FrontShotIsBlocked()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var shieldRam = TestUtils.LoadShieldRam(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        shieldRam.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = TestUtils.GetShot();
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(targetClosestPosition, lastShotPosition, "Shot did not reach the Shield Ram", _currentSeed, SHOT_HIT_DELTA);
        AssertWrapper.AreEqual(ENEMY_HEALTH, TestUtils.GetEnemyHealth(), "Shot from the front hurt the Shield Ram", _currentSeed);
    }

    [UnityTest]
    public IEnumerator BackShotHurts()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var shieldRam = TestUtils.LoadShieldRam(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        shieldRam.transform.rotation = Quaternion.LookRotation(Vector3.forward, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT - halalitPosition);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, TestUtils.GetEnemyHealth(), "Shot from the back did not hurt the Shield Ram exactly once", _currentSeed);
    }

    [UnityTest]
    public IEnumerator KnockbackWaveIgnoresShield()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.KNOCKBACK_WAVE);
        var shieldRam = TestUtils.LoadShieldRam(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        TestUtils.SetRandomEnemyPosition();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        Vector2 shieldRamPosition = shieldRam.transform.position;
        shieldRam.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - shieldRamPosition);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var wave = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        AssertWrapper.IsNotNull(wave, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            wave = GameObject.FindGameObjectWithTag(Tag.KNOCKBACK_WAVE.GetDescription());
        } while (wave != null);

        // THEN
        AssertWrapper.Less(TestUtils.GetEnemyHealth(), ENEMY_HEALTH, "Knockback Wave from the front did not hurt the Shield Ram", _currentSeed);
    }

    // Ofir, 2026-11-25: the shield does not stop Swap Shot. A front hit still swaps; only its damage is blocked.
    [UnityTest]
    public IEnumerator FrontSwapShotStillSwaps()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<SwapShot>(AttackName.SWAP_SHOT, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.SWAP_SHOT);
        var shieldRam = TestUtils.LoadShieldRam(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var halalit = TestUtils.GetHalalit();
        Vector2 halalitStartPosition = halalit.transform.position;
        shieldRam.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitStartPosition - TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var shieldRamStartPosition = TestUtils.GetEnemyPosition();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(shieldRamStartPosition - halalitStartPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);

        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(shieldRamStartPosition, halalit.transform.position, "Halalit was not moved to the Shield Ram's place", _currentSeed, SHOT_HIT_DELTA);
        AssertWrapper.AreEqual(halalitStartPosition, TestUtils.GetEnemyPosition(), "Shield Ram was not moved to the Halalit's place", _currentSeed, SHOT_HIT_DELTA);
        AssertWrapper.AreEqual(ENEMY_HEALTH, TestUtils.GetEnemyHealth(), "Swap Shot from the front hurt the Shield Ram", _currentSeed);
    }

    [UnityTest]
    public IEnumerator EnemyWithoutArmorIsHurtFromFront()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        AssertWrapper.IsNull(enemy.GetComponent<FrontArmor>(), _currentSeed, "Plain enemy has FrontArmor");
        TestUtils.SetEnemyPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // WHEN
        do
        {
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, TestUtils.GetEnemyHealth(), "Shot from the front did not hurt a plain enemy exactly once", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
