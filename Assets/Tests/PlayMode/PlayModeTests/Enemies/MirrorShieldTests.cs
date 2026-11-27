using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MirrorShieldTests
{
    private const int ENEMY_HEALTH = 100;
    private const int REFLECTED_SHOT_POWER = 1;
    private const float REFLECTED_SHOT_AIM_MIN_DOT = 0.9f;
    private const float TURN_DEGREES_PER_SECOND = 90f;
    private const int TURN_FIXED_STEPS = 10;
    private const float TURN_DELTA_DEGREES = 0.5f;
    private const float FULL_TURN_WAIT_SECONDS = 2.2f;
    private const float FACING_DELTA_DEGREES = 1f;
    private static readonly Vector2 SWORD_REACH_POSITION = new(1.5f, 0);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator FrontShotIsReflected()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var mirrorShield = TestUtils.LoadMirrorShield(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        yield return FireAtEnemy(halalitPosition);

        // WHEN
        yield return WaitUntilShotIsGone();
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH, TestUtils.GetEnemyHealth(), "Shot from the front hurt the Mirror Shield", _currentSeed);
        var enemyShot = TestUtils.GetEnemyShot();
        AssertWrapper.IsNotNull(enemyShot, _currentSeed, "Shot from the front was not sent back");
        Vector2 enemyShotPosition = enemyShot.transform.position;
        var enemyShotDirection = enemyShot.GetComponent<Rigidbody2D>().velocity.normalized;
        var directionToHalalit = (halalitPosition - enemyShotPosition).normalized;
        AssertWrapper.Greater(Vector2.Dot(enemyShotDirection, directionToHalalit), REFLECTED_SHOT_AIM_MIN_DOT, "Sent back shot does not fly at the Halalit", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ReflectedShotHurtsShip()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var mirrorShield = TestUtils.LoadMirrorShield(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var halalitHealth = TestUtils.GetHalalit().GetComponent<Health>();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var halalitHealthBefore = halalitHealth.GetHealth();
        yield return FireAtEnemy(halalitPosition);
        yield return WaitUntilShotIsGone();
        yield return new WaitForFixedUpdate();

        var enemyShot = TestUtils.GetEnemyShot();
        AssertWrapper.IsNotNull(enemyShot, _currentSeed, "Shot from the front was not sent back");

        // WHEN
        do
        {
            yield return null;
            enemyShot = TestUtils.GetEnemyShot();
        } while (enemyShot != null);

        // THEN
        AssertWrapper.AreEqual(halalitHealthBefore - REFLECTED_SHOT_POWER, halalitHealth.GetHealth(), "Sent back shot did not hurt the Halalit exactly once", _currentSeed);
    }

    [UnityTest]
    public IEnumerator BackShotHurts()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var mirrorShield = TestUtils.LoadMirrorShield(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT - halalitPosition);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        yield return FireAtEnemy(halalitPosition);

        // WHEN
        yield return WaitUntilShotIsGone();
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, TestUtils.GetEnemyHealth(), "Shot from the back did not hurt the Mirror Shield exactly once", _currentSeed);
        AssertWrapper.IsNull(TestUtils.GetEnemyShot(), _currentSeed, "Shot from the back was sent back");
    }

    [UnityTest]
    public IEnumerator SwordAtFrontHurts()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertMeleeAttack(AttackName.SWORD, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.SWORD);
        var mirrorShield = TestUtils.LoadMirrorShield(SWORD_REACH_POSITION, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - SWORD_REACH_POSITION);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        yield return FireAtEnemy(halalitPosition);
        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.Less(TestUtils.GetEnemyHealth(), ENEMY_HEALTH, "Sword at the front did not hurt the Mirror Shield", _currentSeed);
        AssertWrapper.IsNull(TestUtils.GetEnemyShot(), _currentSeed, "Sword at the front was sent back");
    }

    // Shield Ram keeps its plate in block mode: the Sword, now marked melee, is still blocked at its front.
    [UnityTest]
    public IEnumerator ShieldRamStillBlocksSword()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertMeleeAttack(AttackName.SWORD, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.SWORD);
        var shieldRam = TestUtils.LoadShieldRam(SWORD_REACH_POSITION, _currentSeed);
        AssertWrapper.IsFalse(shieldRam.GetComponent<FrontArmor>().IsReflecting(), "Shield Ram's FrontArmor reflects", _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        shieldRam.transform.rotation = Quaternion.LookRotation(Vector3.forward, halalitPosition - SWORD_REACH_POSITION);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        yield return FireAtEnemy(halalitPosition);
        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH, TestUtils.GetEnemyHealth(), "Sword at the front hurt the Shield Ram", _currentSeed);
        AssertWrapper.IsNull(TestUtils.GetEnemyShot(), _currentSeed, "Shield Ram sent a shot back");
    }

    [UnityTest]
    public IEnumerator TurnsAtNinetyDegreesPerSecond()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var mirrorShield = TestUtils.LoadMirrorShield(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT - halalitPosition);
        yield return null;

        var startRotation = mirrorShield.transform.rotation;

        // WHEN
        mirrorShield.GetComponent<TargetFacer>().enabled = true;
        for (int i = 0; i < TURN_FIXED_STEPS; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        var expectedTurn = TURN_DEGREES_PER_SECOND * Time.fixedDeltaTime * TURN_FIXED_STEPS;
        AssertWrapper.AreEqual(expectedTurn, Quaternion.Angle(startRotation, mirrorShield.transform.rotation), "Mirror Shield did not turn at 90 degrees per second", _currentSeed, TURN_DELTA_DEGREES);

        // WHEN
        yield return new WaitForSeconds(FULL_TURN_WAIT_SECONDS);

        // THEN
        Vector2 mirrorShieldPosition = mirrorShield.transform.position;
        var facingError = Vector2.Angle(mirrorShield.transform.up, halalitPosition - mirrorShieldPosition);
        AssertWrapper.AreEqual(0, facingError, "Mirror Shield does not face the Halalit", _currentSeed, FACING_DELTA_DEGREES);
    }

    // Wounded Retreat pauses TargetFacer, so a fleeing Mirror Shield does not keep its mirror towards the Halalit (#221).
    [UnityTest]
    public IEnumerator FleeingStopsFacing()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var mirrorShield = TestUtils.LoadMirrorShield(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var woundedRetreat = TestUtils.GetWoundedRetreat(mirrorShield);
        var targetFacer = mirrorShield.GetComponent<TargetFacer>();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, TestUtils.DEFAULT_POSITION_TO_THE_RIGHT - halalitPosition);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        targetFacer.enabled = true;
        var startRotation = mirrorShield.transform.rotation;

        // WHEN
        mirrorShield.GetComponent<Health>().TryChangeHealth(-(ENEMY_HEALTH - woundedRetreat.GetWoundedHealth()));
        for (int i = 0; i < TURN_FIXED_STEPS; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "Wounding hit did not start the Mirror Shield's retreat", _currentSeed);
        AssertWrapper.IsFalse(targetFacer.enabled, "TargetFacer was not paused during the retreat", _currentSeed);
        AssertWrapper.Less(Quaternion.Angle(startRotation, mirrorShield.transform.rotation), TURN_DELTA_DEGREES, "Fleeing Mirror Shield kept turning towards the Halalit", _currentSeed);
    }

    private IEnumerator FireAtEnemy(Vector2 halalitPosition)
    {
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition - halalitPosition, weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        AssertWrapper.IsNotNull(TestUtils.GetShot(), _currentSeed, "Attack was not fired");
    }

    private IEnumerator WaitUntilShotIsGone()
    {
        GameObject shot;
        do
        {
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
