using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class DecoyShipTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.DECOY_SHIP;
    private const int ENEMY_HEALTH = 100;
    private const float POSITION_DELTA = 0.01f;
    private const float DECOY_DISTANCE_FROM_HALALIT = 4f;
    private const int CHARGE_FIXED_UPDATES = 5;
    private const float SAME_DIRECTION_DOT = 0.9f;
    private static readonly Vector2 CHARGER_OFFSET_FROM_HALALIT = new(0, 5);
    private static readonly Vector2 FIRST_ENEMY_OFFSET_FROM_HALALIT = new(3, 0);
    private static readonly Vector2 SECOND_ENEMY_OFFSET_FROM_HALALIT = new(6, 0);

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator TargetIsShipWithoutDecoy()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<DecoyShip>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        yield return null;

        // THEN
        AssertWrapper.IsNull(TestUtils.GetShot(), _currentSeed, "There is a shot before firing");
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(), "Enemy target is not the Halalit without a decoy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NewestDecoyIsTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<DecoyShip>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return null;

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var firstDecoy = TestUtils.GetShot();
        AssertWrapper.IsNotNull(firstDecoy, _currentSeed, "First decoy was not fired");

        // WHEN
        while (!weaponAttack.HumbleFixedUpdate(touchOnJoystick))
        {
            yield return null;
        }
        yield return null;

        // THEN
        GameObject secondDecoy = null;
        foreach (var shot in GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription()))
        {
            if (shot != firstDecoy)
            {
                secondDecoy = shot;
            }
        }
        AssertWrapper.IsTrue(firstDecoy != null, "First decoy was gone before the second was fired", _currentSeed);
        AssertWrapper.IsNotNull(secondDecoy, _currentSeed, "Second decoy was not fired");
        AssertWrapper.AreEqual(secondDecoy.transform.position, EnemyTarget.GetPosition(), "Enemy target is not the newest decoy", _currentSeed, POSITION_DELTA);

        // WHEN
        do
        {
            yield return null;
        } while (TestUtils.GetShot() != null);

        yield return null;

        // THEN
        AssertWrapper.AreEqual(Utils.GetHalalitPosition(), EnemyTarget.GetPosition(), "Enemy target did not go back to the Halalit after the decoys were gone", _currentSeed, POSITION_DELTA);
    }

    [UnityTest]
    public IEnumerator ChargerChargesDecoy()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<DecoyShip>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var charger = TestUtils.LoadEnemyWithoutStateMachine(TestUtils.FOLLOWING_ENEMY_RADIUS_PREFAB_PATH, halalitPosition + CHARGER_OFFSET_FROM_HALALIT, _currentSeed);
        yield return null;

        var chargerAttack = charger.GetComponent<WaitAttackFollowingAttack>();
        var chargerRigidBody = charger.GetComponent<Rigidbody2D>();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var decoy = TestUtils.GetShot();
        AssertWrapper.IsNotNull(decoy, _currentSeed, "Decoy was not fired");

        // WHEN
        while (decoy != null && Vector2.Distance(decoy.transform.position, halalitPosition) < DECOY_DISTANCE_FROM_HALALIT)
        {
            yield return null;
            decoy = TestUtils.GetShot();
        }

        // THEN
        AssertWrapper.IsNotNull(decoy, _currentSeed, "Decoy was gone before it flew away from the Halalit");

        // GIVEN
        Vector2 chargerPosition = charger.transform.position;
        Vector2 decoyPosition = decoy.transform.position;
        var directionToDecoy = (decoyPosition - chargerPosition).normalized;
        var directionToHalalit = (halalitPosition - chargerPosition).normalized;
        AssertWrapper.Less(Vector2.Dot(directionToDecoy, directionToHalalit), SAME_DIRECTION_DOT, "Test setup: the decoy and the Halalit are in the same direction from the charger", _currentSeed);

        // WHEN
        chargerAttack.SetAttacking();
        for (int i = 0; i < CHARGE_FIXED_UPDATES; i++)
        {
            chargerAttack.Attack();
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.Greater(chargerRigidBody.velocity.magnitude, 0, "Charger did not move", _currentSeed);
        AssertWrapper.Greater(Vector2.Dot(chargerRigidBody.velocity.normalized, directionToDecoy), SAME_DIRECTION_DOT, "Charger did not charge at the decoy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PassesThroughTwoEnemies()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<DecoyShip>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var firstEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        var secondEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        TestUtils.SetGameObjectPosition(firstEnemy, halalitPosition + FIRST_ENEMY_OFFSET_FROM_HALALIT);
        TestUtils.SetGameObjectPosition(secondEnemy, halalitPosition + SECOND_ENEMY_OFFSET_FROM_HALALIT);
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        yield return null;

        var firstEnemyHealth = firstEnemy.GetComponent<Health>();
        var secondEnemyHealth = secondEnemy.GetComponent<Health>();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(FIRST_ENEMY_OFFSET_FROM_HALALIT, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var decoy = TestUtils.GetShot();
        AssertWrapper.IsNotNull(decoy, _currentSeed, "Decoy was not fired");

        // WHEN
        while (decoy != null && firstEnemyHealth.GetHealth() == ENEMY_HEALTH)
        {
            yield return null;
            decoy = TestUtils.GetShot();
        }

        // A shot destroyed on the hit still exists until the end of that frame: wait one frame before asking whether it lives.
        yield return null;
        decoy = TestUtils.GetShot();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, firstEnemyHealth.GetHealth(), "Decoy did not hurt the first enemy exactly once", _currentSeed);
        AssertWrapper.IsNotNull(decoy, _currentSeed, "Decoy did not pass through the first enemy");

        // WHEN
        do
        {
            yield return null;
        } while (TestUtils.GetShot() != null);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, firstEnemyHealth.GetHealth(), "Decoy hurt the first enemy more than once", _currentSeed);
        AssertWrapper.AreEqual(ENEMY_HEALTH - TestUtils.DEFAULT_POWER_1, secondEnemyHealth.GetHealth(), "Decoy did not hurt the second enemy exactly once", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
