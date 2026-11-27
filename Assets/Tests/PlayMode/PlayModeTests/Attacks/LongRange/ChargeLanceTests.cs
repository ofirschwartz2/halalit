using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ChargeLanceTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.CHARGE_LANCE;
    private const int TARGET_HEALTH = 100;
    private const int FULL_CHARGE_LEVEL = 3;
    private const int FULL_HARM_MULTIPLIER = 3;
    private const int LINE_TEST_ENEMIES = 5;
    // Hold times, away from the shortest hold (0.2 s) and the glow steps (0.4, 0.8 and 1.2 s).
    private const float TAP_TIME = 0.1f;
    private const float PARTIAL_HOLD_TIME = 0.25f;
    private const float FIRST_STEP_HOLD_TIME = 0.6f;
    private const float FULL_HOLD_TIME = 1.5f;
    private const float BEFORE_FIRST_GLOW_TIME = 0.2f;
    private const float AFTER_FIRST_GLOW_TIME = 0.5f;
    private const float COOLDOWN_OVERHOLD = 0.05f;
    // x1 at no glow, rising to x3 in three steps. Harm multipliers round up.
    private const float FIRST_STEP_HARM_MULTIPLIER = 5f / 3f;
    // Distances along the aim, from the weapon. A partial lance is 4 long and a full one 8.
    private const float FIRST_TARGET_DISTANCE = 1.5f;
    private const float SECOND_TARGET_DISTANCE = 2.5f;
    private const float THIRD_TARGET_DISTANCE = 3.5f;
    private const float OFF_LINE_DISTANCE = 2.5f;
    private const float BEYOND_PARTIAL_LANCE_DISTANCE = 5.5f;
    private const float FULL_LANCE_TARGET_DISTANCE = 7f;
    private const float BEYOND_FULL_LANCE_DISTANCE = 9.5f;
    // A loaded enemy is 1 wide along the line. At 5 its near side (4.5) is past a no-glow lance (4) and inside a first-step
    // lance (5.3). At 6.5 its near side (6) is past a first-step lance and inside a second-step lance (6.7).
    private const float FIRST_STEP_TARGET_DISTANCE = 5f;
    private const float BEYOND_FIRST_STEP_DISTANCE = 6.5f;
    private const float BLOCKER_DISTANCE = 2f;
    private const float BEHIND_BLOCKER_DISTANCE = 3.5f;
    // A rock this far out leaves room for an enemy at FIRST_TARGET_DISTANCE between it and the weapon.
    private const float FAR_BLOCKER_DISTANCE = 3f;
    private const float PARKING_DISTANCE = 6f;
    private const float LIFETIME_MARGIN = 0.1f;
    private const float FORCE_DELTA = 0.001f;
    // The Leech's slow-down in the offer.
    private const float FAKE_LEECH_THRUST_FACTOR = 0.7f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator LanceIsWired()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);

        // WHEN
        var lancePrefab = AttacksBank.GetAttackPrefab(ATTACK_NAME);
        var attackBehaviour = lancePrefab.GetComponent<AttackBehaviour>();
        var boxCollider = lancePrefab.GetComponent<BoxCollider2D>();

        // THEN
        AssertWrapper.IsTrue(attackBehaviour != null && attackBehaviour.ShotType == AttackShotType.DESCRETE, "Charge Lance Prefab's AttackBehaviour Is Not DESCRETE (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.IsTrue(lancePrefab.GetComponent<DescreteTriggerHarmer>() != null, "Charge Lance Prefab Has No DescreteTriggerHarmer (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.IsTrue(boxCollider != null && boxCollider.isTrigger, "Charge Lance Prefab Has No Trigger BoxCollider2D (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.IsTrue(lancePrefab.GetComponent<LineRenderer>() != null, "Charge Lance Prefab Has No LineRenderer (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.IsTrue(lancePrefab.GetComponent<Rigidbody2D>() == null, "Charge Lance Prefab Has A Rigidbody2D (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.IsTrue(lancePrefab.CompareTag(Tag.SHOT.GetDescription()), "Charge Lance Prefab Is Not Tagged Shot (Needs Ofir in Unity, #236)", _currentSeed);
        AssertWrapper.AreEqual(LayerMask.NameToLayer(Layer.PLAYER_SHOTS.GetDescription()), lancePrefab.layer, "Charge Lance Prefab Is Not On The PlayerShots Layer (Needs Ofir in Unity, #236)", _currentSeed);
        // The box is sized in the lance's own units, the line and the stop check in world units: they match only at scale 1.
        AssertWrapper.IsTrue((Vector2)lancePrefab.transform.localScale == Vector2.one, "Charge Lance Prefab's Scale Is Not 1, So Its Hit Box Would Not Match Its Line (Needs Ofir in Unity, #236)", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TapFiresNothing()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);

        // WHEN
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, TAP_TIME);
        var didTapFire = weaponAttack.HumbleFixedUpdate(Vector2.zero);
        var isLanceAfterTap = Object.FindObjectOfType<ChargeLance>() != null;

        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);
        var didHoldFire = weaponAttack.HumbleFixedUpdate(Vector2.zero);

        // THEN
        AssertWrapper.IsFalse(didTapFire, "A Tap Fired A Lance", _currentSeed);
        AssertWrapper.IsFalse(isLanceAfterTap, "A Tap Left A Lance In The Scene", _currentSeed);
        AssertWrapper.IsTrue(didHoldFire, "A Hold Right After A Tap Did Not Fire A Lance", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PartialLanceHitsLineOnce()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var enemies = GetObjectLoader().LoadEnemiesInExternalSafeIsland(LINE_TEST_ENEMIES, _currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(enemies[0], weaponPosition, FIRST_TARGET_DISTANCE);
        PlaceOnLine(enemies[1], weaponPosition, SECOND_TARGET_DISTANCE);
        PlaceOnLine(enemies[2], weaponPosition, THIRD_TARGET_DISTANCE);
        TestUtils.SetGameObjectPosition(enemies[3], weaponPosition + Vector2.right * SECOND_TARGET_DISTANCE + Vector2.up * OFF_LINE_DISTANCE);
        PlaceOnLine(enemies[4], weaponPosition, BEYOND_PARTIAL_LANCE_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        var firstHealthAfterHit = GetHealth(enemies[0]);
        var secondHealthAfterHit = GetHealth(enemies[1]);
        var thirdHealthAfterHit = GetHealth(enemies[2]);

        yield return WaitForLanceEnd(lance);

        // THEN
        var hitHealth = TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power;
        AssertWrapper.AreEqual(hitHealth, firstHealthAfterHit, "A Partial Lance Did Not Hit The First Enemy On The Line Once", _currentSeed);
        AssertWrapper.AreEqual(hitHealth, secondHealthAfterHit, "A Partial Lance Did Not Hit The Second Enemy On The Line Once", _currentSeed);
        AssertWrapper.AreEqual(hitHealth, thirdHealthAfterHit, "A Partial Lance Did Not Hit The Third Enemy On The Line Once", _currentSeed);
        AssertWrapper.AreEqual(hitHealth, GetHealth(enemies[0]), "A Partial Lance Hit The First Enemy Again During Its Lifetime", _currentSeed);
        AssertWrapper.AreEqual(hitHealth, GetHealth(enemies[1]), "A Partial Lance Hit The Second Enemy Again During Its Lifetime", _currentSeed);
        AssertWrapper.AreEqual(hitHealth, GetHealth(enemies[2]), "A Partial Lance Hit The Third Enemy Again During Its Lifetime", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemies[3]), "A Partial Lance Hit An Enemy Beside The Line", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemies[4]), "A Partial Lance Hit An Enemy Beyond Its Length", _currentSeed);
    }

    // The lance hurts only what is on its line when it is let go. An enemy that moves onto the line while the line still shows
    // is not hurt. This keeps an advancing Mirror Shield from touching a partial lance and sending it back.
    [UnityTest]
    public IEnumerator LanceHitsOnlyWhatIsOnLineAtRelease()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var enemyOnLine = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        var lateEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(enemyOnLine, weaponPosition, FIRST_TARGET_DISTANCE);
        TestUtils.SetGameObjectPosition(lateEnemy, weaponPosition + Vector2.right * THIRD_TARGET_DISTANCE + Vector2.up * OFF_LINE_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        // Two physics steps: the step that hits, and the step in which the lance switches its box off.
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        var isLanceShowing = lance != null;
        PlaceOnLine(lateEnemy, weaponPosition, THIRD_TARGET_DISTANCE);
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.IsTrue(isLanceShowing, "Test Setup: The Lance Was Gone Before The Late Enemy Moved Onto Its Line", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power, GetHealth(enemyOnLine), "Test Setup: The Lance Did Not Hit The Enemy On Its Line Once", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(lateEnemy), "A Lance Hurt An Enemy That Moved Onto Its Line After The Release", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FullLanceReachesFarther()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var reachedEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        var beyondEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(reachedEnemy, weaponPosition, FULL_LANCE_TARGET_DISTANCE);
        PlaceOnLine(beyondEnemy, weaponPosition, BEYOND_FULL_LANCE_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(reachedEnemy), "A Full Lance Did Not Hit An Enemy 7 Away Once With Three Times The Power", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(beyondEnemy), "A Full Lance Hit An Enemy Beyond Its Length", _currentSeed);
    }

    // Harm and length rise in the glow steps, not smoothly: a lance let go between the first (0.4 s) and second (0.8 s) glow
    // hits for x1.67 and is 5.3 long.
    [UnityTest]
    public IEnumerator FirstGlowStepRaisesHarmAndReach()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var reachedEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        var beyondEnemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FIRST_STEP_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(reachedEnemy, weaponPosition, FIRST_STEP_TARGET_DISTANCE);
        PlaceOnLine(beyondEnemy, weaponPosition, BEYOND_FIRST_STEP_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return WaitForLanceEnd(lance);

        // THEN
        var firstStepHarm = Mathf.CeilToInt(TestUtils.DEFAULT_ATTACK_STATS_2.Power * FIRST_STEP_HARM_MULTIPLIER);
        AssertWrapper.AreEqual(TARGET_HEALTH - firstStepHarm, GetHealth(reachedEnemy), "A Lance At The First Glow Step Did Not Hit An Enemy 5 Away Once With 1.67 Times The Power", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(beyondEnemy), "A Lance At The First Glow Step Reached An Enemy Beyond Its Length", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PartialLanceStopsShortOfRock()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var rock = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(TARGET_HEALTH);
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceRockOnLine(rock, weaponPosition, BLOCKER_DISTANCE);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        var lanceLength = lance.GetLength();
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.Less(lanceLength, BLOCKER_DISTANCE, "A Partial Lance Did Not Stop Short Of The Rock", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(rock), "A Partial Lance Hurt The Rock", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemy), "A Partial Lance Hit The Enemy Behind The Rock", _currentSeed);
    }

    // "Just short": a partial lance stops at the first rock, not before it, so it still hits an enemy between the weapon and the rock.
    [UnityTest]
    public IEnumerator PartialLanceStillHitsEnemyBeforeRock()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var rock = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(TARGET_HEALTH);
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(enemy, weaponPosition, FIRST_TARGET_DISTANCE);
        PlaceRockOnLine(rock, weaponPosition, FAR_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        var lanceLength = lance.GetLength();
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.Less(lanceLength, FAR_BLOCKER_DISTANCE, "Test Setup: The Partial Lance Did Not Stop At The Rock", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power, GetHealth(enemy), "A Partial Lance Stopped Too Early To Hit The Enemy In Front Of The Rock", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(rock), "A Partial Lance Hurt The Rock", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FullLanceGoesThroughRock()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GetObjectLoader();
        var rock = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(TARGET_HEALTH);
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceRockOnLine(rock, weaponPosition, BLOCKER_DISTANCE);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(rock), "A Full Lance Did Not Hurt The Rock Once", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(enemy), "A Full Lance Did Not Hit The Enemy Behind The Rock Once", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ChargingSlowsShip()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var weaponSprite = weaponAttack.GetComponent<SpriteRenderer>();
        var baseColor = weaponSprite.color;
        var halalitMovement = TestUtils.GetHalalitMovement();
        var chargingThrustFactor = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<ChargeLance>().GetChargingThrustFactor();
        var touchOnJoystick = AimRight(weaponAttack);
        var freeForce = TestUtils.PushHalalitOnce(halalitMovement);
        AssertWrapper.Greater(freeForce.magnitude, 0, "Test Setup: The Halalit Pushed With No Force", _currentSeed);
        AssertWrapper.Less(chargingThrustFactor, 1, "Test Setup: Charging Does Not Slow The Ship", _currentSeed);

        // WHEN
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);
        var chargingForce = TestUtils.PushHalalitOnce(halalitMovement);
        var chargingColor = weaponSprite.color;
        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        var releasedForce = TestUtils.PushHalalitOnce(halalitMovement);

        // THEN
        AssertWrapper.IsTrue(chargingColor != baseColor, "Test Setup: The Weapon Did Not Glow While Charging", _currentSeed);
        AssertWrapper.AreEqual(freeForce * chargingThrustFactor, chargingForce, "Charging Did Not Scale The Thrust By Its Factor", _currentSeed, FORCE_DELTA);
        AssertWrapper.AreEqual(freeForce, releasedForce, "The Thrust Did Not Come Back After The Release", _currentSeed, FORCE_DELTA);
        AssertWrapper.IsTrue(weaponSprite.color == baseColor, "The Weapon Colour Did Not Come Back After The Release", _currentSeed);
    }

    // A latched Leech (x0.7) and charging (x0.6) stack to x0.42. Letting go ends only the lance's slow-down, not the Leech's.
    [UnityTest]
    public IEnumerator ChargingStacksWithLatchedLeech()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var halalitMovement = TestUtils.GetHalalitMovement();
        var chargingThrustFactor = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<ChargeLance>().GetChargingThrustFactor();
        var touchOnJoystick = AimRight(weaponAttack);
        var freeForce = TestUtils.PushHalalitOnce(halalitMovement);
        AssertWrapper.Greater(freeForce.magnitude, 0, "Test Setup: The Halalit Pushed With No Force", _currentSeed);
        AssertWrapper.Less(chargingThrustFactor, 1, "Test Setup: Charging Does Not Slow The Ship", _currentSeed);
        var fakeLeech = new GameObject("FakeLeech");
        AssertWrapper.IsTrue(ShipLatches.TryLatch(fakeLeech.transform, FAKE_LEECH_THRUST_FACTOR), "Test Setup: A Free Slot Refused The Latch", _currentSeed);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);
        AssertWrapper.IsTrue(weaponAttack.IsCharging(), "Test Setup: The Weapon Is Not Charging", _currentSeed);

        // WHEN
        var chargingLatchedForce = TestUtils.PushHalalitOnce(halalitMovement);
        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        var releasedLatchedForce = TestUtils.PushHalalitOnce(halalitMovement);

        // THEN
        AssertWrapper.AreEqual(freeForce * FAKE_LEECH_THRUST_FACTOR * chargingThrustFactor, chargingLatchedForce, "Charging With A Latched Leech Did Not Slow The Thrust By Both Factors", _currentSeed, FORCE_DELTA);
        AssertWrapper.AreEqual(freeForce * FAKE_LEECH_THRUST_FACTOR, releasedLatchedForce, "After The Release The Thrust Did Not Go Back To The Leech's Slow-Down Alone", _currentSeed, FORCE_DELTA);
    }

    [UnityTest]
    public IEnumerator SwitchingWeaponEndsCharge()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var weaponSprite = weaponAttack.GetComponent<SpriteRenderer>();
        var baseColor = weaponSprite.color;
        var halalitMovement = TestUtils.GetHalalitMovement();
        var touchOnJoystick = AimRight(weaponAttack);
        var freeForce = TestUtils.PushHalalitOnce(halalitMovement);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);
        AssertWrapper.IsTrue(weaponAttack.IsCharging(), "Test Setup: The Weapon Is Not Charging", _currentSeed);
        AssertWrapper.IsTrue(weaponSprite.color != baseColor, "Test Setup: The Weapon Did Not Glow While Charging", _currentSeed);

        // WHEN
        TestUtils.SetUpShot(AttackName.BALL_SHOT, TestUtils.DEFAULT_ATTACK_STATS_2);
        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        var forceAfterSwitch = TestUtils.PushHalalitOnce(halalitMovement);

        // THEN
        AssertWrapper.IsFalse(weaponAttack.IsCharging(), "Switching Weapon Did Not End The Charge", _currentSeed);
        AssertWrapper.AreEqual(freeForce, forceAfterSwitch, "The Thrust Did Not Come Back After Switching Weapon", _currentSeed, FORCE_DELTA);
        AssertWrapper.IsTrue(weaponSprite.color == baseColor, "The Weapon Colour Did Not Come Back After Switching Weapon", _currentSeed);
        AssertWrapper.IsTrue(Object.FindObjectOfType<ChargeLance>() == null, "A Lance Was Fired After Switching Weapon", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PartialLanceStopsShortOfMirrorShield()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var mirrorShield = TestUtils.LoadMirrorShield(GetParkingPosition(), _currentSeed);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(mirrorShield, weaponPosition, BLOCKER_DISTANCE);
        FaceWeapon(mirrorShield, weaponPosition);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        var lanceLength = lance.GetLength();
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.Less(lanceLength, BLOCKER_DISTANCE, "A Partial Lance Did Not Stop Short Of A Mirror Shield Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(mirrorShield), "A Partial Lance Hurt A Mirror Shield Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemy), "A Partial Lance Hit The Enemy Behind A Mirror Shield Facing It", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetEnemyShot() == null, "A Mirror Shield Sent A Partial Lance Back", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FullLanceBreaksMirrorShield()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var mirrorShield = TestUtils.LoadMirrorShield(GetParkingPosition(), _currentSeed);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(mirrorShield, weaponPosition, BLOCKER_DISTANCE);
        FaceWeapon(mirrorShield, weaponPosition);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(mirrorShield), "A Full Lance Did Not Break Through A Mirror Shield Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(enemy), "A Full Lance Did Not Hit The Enemy Behind The Mirror Shield", _currentSeed);
        AssertWrapper.IsTrue(TestUtils.GetEnemyShot() == null, "A Mirror Shield Sent A Full Lance Back", _currentSeed);
    }

    // The Shield Ram's plate blocks rather than reflects. A partial lance stops short of it, as it does of a Mirror Shield.
    [UnityTest]
    public IEnumerator PartialLanceStopsShortOfShieldRam()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var shieldRam = TestUtils.LoadShieldRam(GetParkingPosition(), _currentSeed);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(shieldRam, weaponPosition, BLOCKER_DISTANCE);
        FaceWeapon(shieldRam, weaponPosition);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        var lanceLength = lance.GetLength();
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.Less(lanceLength, BLOCKER_DISTANCE, "A Partial Lance Did Not Stop Short Of A Shield Ram Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(shieldRam), "A Partial Lance Hurt A Shield Ram Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemy), "A Partial Lance Hit The Enemy Behind A Shield Ram Facing It", _currentSeed);
    }

    // Only a full lance breaks a plate: the Shield Ram's too, since it uses the same FrontArmor as the Mirror Shield.
    [UnityTest]
    public IEnumerator FullLanceBreaksShieldRamPlate()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var shieldRam = TestUtils.LoadShieldRam(GetParkingPosition(), _currentSeed);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME);

        Vector2 weaponPosition = weaponAttack.transform.position;
        PlaceOnLine(shieldRam, weaponPosition, BLOCKER_DISTANCE);
        FaceWeapon(shieldRam, weaponPosition);
        PlaceOnLine(enemy, weaponPosition, BEHIND_BLOCKER_DISTANCE);
        yield return new WaitForFixedUpdate();

        // WHEN
        var lance = ReleaseLance(weaponAttack);
        yield return WaitForLanceEnd(lance);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(shieldRam), "A Full Lance Did Not Break Through A Shield Ram Facing It", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - GetFullHarm(), GetHealth(enemy), "A Full Lance Did Not Hit The Enemy Behind The Shield Ram", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ChargeWaitsForCooldown()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_1);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var minCooldown = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<AttackBehaviour>().MinCooldown;
        var cooldown = Mathf.Max(TestUtils.DEFAULT_ATTACK_STATS_1.Rate * weaponAttack.GetCooldownInterval(), minCooldown);
        var touchOnJoystick = AimRight(weaponAttack);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);
        AssertWrapper.IsTrue(weaponAttack.HumbleFixedUpdate(Vector2.zero), "Test Setup: The First Lance Was Not Fired", _currentSeed);

        // WHEN
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, cooldown + COOLDOWN_OVERHOLD);
        var didFireThroughCooldown = weaponAttack.HumbleFixedUpdate(Vector2.zero);
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, PARTIAL_HOLD_TIME);
        var didFireAfterCooldown = weaponAttack.HumbleFixedUpdate(Vector2.zero);

        // THEN
        AssertWrapper.IsFalse(didFireThroughCooldown, "A Hold Through The Wait Fired: The Charge Counted Before The Wait Was Over", _currentSeed);
        AssertWrapper.IsTrue(didFireAfterCooldown, "A Hold After The Wait Did Not Fire A Lance", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WeaponGlowsInSteps()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChargeLance>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var weaponSprite = weaponAttack.GetComponent<SpriteRenderer>();
        var baseColor = weaponSprite.color;
        var chargeLance = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<ChargeLance>();
        var touchOnJoystick = AimRight(weaponAttack);

        // WHEN
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, BEFORE_FIRST_GLOW_TIME);
        var colorBeforeFirstStep = weaponSprite.color;
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, AFTER_FIRST_GLOW_TIME - BEFORE_FIRST_GLOW_TIME);
        var colorAfterFirstStep = weaponSprite.color;
        yield return TestUtils.HoldAttack(weaponAttack, touchOnJoystick, FULL_HOLD_TIME - AFTER_FIRST_GLOW_TIME);
        var colorAtFullCharge = weaponSprite.color;
        weaponAttack.HumbleFixedUpdate(Vector2.zero);
        var colorAfterRelease = weaponSprite.color;

        // THEN
        AssertWrapper.IsTrue(colorBeforeFirstStep == baseColor, "The Weapon Glowed Before The First Charge Step", _currentSeed);
        AssertWrapper.IsTrue(colorAfterFirstStep != baseColor, "The Weapon Did Not Glow After The First Charge Step", _currentSeed);
        AssertWrapper.IsTrue(colorAtFullCharge == chargeLance.GetChargeColor(baseColor, FULL_CHARGE_LEVEL), "The Weapon Did Not Glow In The Full Charge Colour", _currentSeed);
        AssertWrapper.IsTrue(colorAfterRelease == baseColor, "The Weapon Colour Did Not Come Back After The Release", _currentSeed);
    }

    private Bounds GetWorldBounds()
    {
        return TestUtils.GetInternalWorldBoxCollider2D().bounds;
    }

    private ObjectLoader GetObjectLoader()
    {
        return GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
    }

    // Inside the world, away from the ship and from the line the lance fires along.
    private Vector2 GetParkingPosition()
    {
        return (Vector2)GetWorldBounds().center + Vector2.down * PARKING_DISTANCE;
    }

    private IEnumerator PlaceHalalit(Vector2 position)
    {
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), position);
        yield return new WaitForFixedUpdate();
        yield return null;
    }

    // Aims the weapon to the right of the ship and returns a touch past the fire edge in that direction.
    private Vector2 AimRight(WeaponAttack weaponAttack)
    {
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(Vector2.right, weaponAttack.GetAttackJoystickEdge());
        TestUtils.GetWeaponMovement().TryChangeWeaponPosition(touchOnJoystick);
        return touchOnJoystick;
    }

    private void PlaceOnLine(GameObject target, Vector2 weaponPosition, float distance)
    {
        TestUtils.SetGameObjectPosition(target, weaponPosition + Vector2.right * distance);
    }

    // A drifting rock is always awake. A loaded rock stands still, so its body may be asleep, and the lance has no Rigidbody2D
    // to wake it: the two would never touch. Waking it makes the rock what it is in a run.
    private void PlaceRockOnLine(GameObject rock, Vector2 weaponPosition, float distance)
    {
        PlaceOnLine(rock, weaponPosition, distance);
        rock.GetComponent<Rigidbody2D>().WakeUp();
    }

    private void FaceWeapon(GameObject enemy, Vector2 weaponPosition)
    {
        enemy.transform.rotation = Quaternion.LookRotation(Vector3.forward, weaponPosition - (Vector2)enemy.transform.position);
    }

    // Lets go of the stick and returns the lance that left the weapon.
    private ChargeLance ReleaseLance(WeaponAttack weaponAttack)
    {
        AssertWrapper.IsTrue(weaponAttack.HumbleFixedUpdate(Vector2.zero), "Test Setup: Letting Go Did Not Fire A Lance", _currentSeed);
        return Object.FindObjectOfType<ChargeLance>();
    }

    private IEnumerator WaitForLanceEnd(ChargeLance lance)
    {
        var endTime = Time.time + lance.GetLifetime() + LIFETIME_MARGIN;
        while (lance != null && Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
    }

    private int GetHealth(GameObject target)
    {
        return target.GetComponent<Health>().GetHealth();
    }

    private int GetFullHarm()
    {
        return TestUtils.DEFAULT_ATTACK_STATS_2.Power * FULL_HARM_MULTIPLIER;
    }

    [TearDown]
    public void TearDown()
    {
        // Latches are static: they outlive the scene.
        ShipLatches.ClearAll();
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
