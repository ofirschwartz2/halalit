using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class TripwireTests
{
    private int _currentSeed;
    private GameObject _firstPost;
    private GameObject _secondPost;
    private const AttackName ATTACK_NAME = AttackName.TRIPWIRE;
    private const int TARGET_HEALTH = 100;
    private const float POSTS_DISTANCE = 4f;
    private const float BEYOND_MAX_LENGTH = 1f;
    private const float OFF_WIRE_DISTANCE = 3f;
    private const float HALALIT_DISTANCE_BELOW_POSTS = 4f;
    private const float FLIGHT_DISTANCE_DELTA = 0.5f;
    private const float TIME_MARGIN = 0.1f;
    private const float PLANT_MARGIN = 0.3f;
    private const float LIFETIME_MARGIN = 0.3f;
    private const float STOPPED_SPEED_DELTA = 0.01f;
    private const float NEAR_ZERO_COOLDOWN_MULTIPLIER = 0.01f;
    private const float HOLD_FIRE_TIME = 1.5f;
    private const float COOLDOWN_DELTA = 0.05f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator PostStopsAfterFlight()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        Vector2 startPosition = GetWorldBounds().center;
        yield return PlaceHalalit(startPosition + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);

        // WHEN
        var post = TestUtils.LoadTripwirePost(startPosition, Vector2.up);
        var tripwirePost = post.GetComponent<TripwirePost>();
        var postRigidBody = post.GetComponent<Rigidbody2D>();
        var lifetime = tripwirePost.GetLifeTime();
        var flightDistance = tripwirePost.GetSpeed() * tripwirePost.GetFlightTime();
        var startTime = Time.time;
        yield return WaitForPlant(tripwirePost);

        Vector2 plantedPosition = post.transform.position;
        float plantedSpeed = postRigidBody.velocity.magnitude;

        while (post != null && Time.time < startTime + lifetime + LIFETIME_MARGIN)
        {
            yield return null;
        }

        // THEN
        AssertWrapper.AreEqual(flightDistance, Vector2.Distance(startPosition, plantedPosition), "Post Did Not Stop At Its Flight Distance", _currentSeed, FLIGHT_DISTANCE_DELTA);
        AssertWrapper.AreEqual(0, plantedSpeed, "Post Still Moves After Its Flight Time", _currentSeed, STOPPED_SPEED_DELTA);
        AssertWrapper.IsTrue(post == null, "Post Was Not Removed After Its Lifetime", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ThirdPostRemovesOldest()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        Vector2 center = GetWorldBounds().center;
        yield return PlaceHalalit(center + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);

        // WHEN
        var firstPost = TestUtils.LoadTripwirePost(center + Vector2.left * POSTS_DISTANCE, Vector2.up);
        yield return null;
        TestUtils.LoadTripwirePost(center, Vector2.up);
        yield return null;
        TestUtils.LoadTripwirePost(center + Vector2.right * POSTS_DISTANCE, Vector2.up);
        yield return null;
        // Lets the middle post's wire switch off, which must not remove the middle post.
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(2, Object.FindObjectsOfType<TripwirePost>().Length, "There Are Not Exactly Two Posts After The Third Shot", _currentSeed);
        AssertWrapper.IsTrue(firstPost == null, "The Oldest Post Was Not Removed By The Third Shot", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WireHurtsEnemyOnce()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        Vector2 center = GetWorldBounds().center;
        yield return PlaceHalalit(center + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);
        yield return PlantTwoPosts(center, POSTS_DISTANCE);
        var wireMiddle = GetWireMiddle();
        var offWire = wireMiddle + Vector2.down * OFF_WIRE_DISTANCE;
        var wireHarmer = GetWireHarmer();
        AssertWrapper.IsNotNull(wireHarmer, _currentSeed, "Wire Has No DescreteTriggerHarmer (Needs Ofir in Unity)");
        var sameTargetCooldown = wireHarmer.GetSameTargetCooldown();
        AssertWrapper.Greater(sameTargetCooldown, 0, "Wire's DescreteTriggerHarmer Has No Same Target Cooldown (Needs Ofir in Unity)", _currentSeed);

        // WHEN
        TestUtils.SetEnemyPosition(wireMiddle);
        yield return new WaitForFixedUpdate();
        var firstContactTime = Time.time;
        var healthAfterFirstContact = TestUtils.GetEnemyHealth();

        TestUtils.SetEnemyPosition(offWire);
        yield return new WaitForFixedUpdate();
        TestUtils.SetEnemyPosition(wireMiddle);
        yield return new WaitForFixedUpdate();
        var quickReturnTime = Time.time;
        var healthAfterQuickReturn = TestUtils.GetEnemyHealth();

        TestUtils.SetEnemyPosition(offWire);
        yield return new WaitForFixedUpdate();
        while (Time.time < firstContactTime + sameTargetCooldown + TIME_MARGIN)
        {
            yield return new WaitForFixedUpdate();
        }
        TestUtils.SetEnemyPosition(wireMiddle);
        yield return new WaitForFixedUpdate();
        var healthAfterLateReturn = TestUtils.GetEnemyHealth();

        // THEN
        AssertWrapper.Less(quickReturnTime, firstContactTime + sameTargetCooldown, "Test Setup: The Quick Return Came After The Same Target Cooldown", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power, healthAfterFirstContact, "Wire Did Not Hurt The Enemy Once On Contact", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power, healthAfterQuickReturn, "Wire Hurt The Enemy Again Within Its Same Target Cooldown", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - 2 * TestUtils.DEFAULT_ATTACK_STATS_2.Power, healthAfterLateReturn, "Wire Did Not Hurt The Enemy Again After Its Same Target Cooldown", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WireHurtsAsteroid()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(TARGET_HEALTH);
        Vector2 center = GetWorldBounds().center;
        yield return PlaceHalalit(center + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);
        yield return PlantTwoPosts(center, POSTS_DISTANCE);

        // WHEN
        TestUtils.SetAsteroidPosition(GetWireMiddle());
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - TestUtils.DEFAULT_ATTACK_STATS_2.Power, TestUtils.GetAsteroidHealth(), "Wire Did Not Hurt The Asteroid Once (Needs Ofir in Unity: Wire tagged Shot?)", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NoWireBeyondMaxLength()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        Vector2 center = GetWorldBounds().center;
        yield return PlaceHalalit(center + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);
        var maxLength = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<TripwirePost>().GetWire().GetMaxLength();
        yield return PlantTwoPosts(center, maxLength + BEYOND_MAX_LENGTH);

        // WHEN
        TestUtils.SetEnemyPosition(GetWireMiddle());
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsFalse(_secondPost.GetComponent<TripwirePost>().GetWire().IsWireOn(), "Wire Is On Between Posts Farther Apart Than Its Max Length", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, TestUtils.GetEnemyHealth(), "Enemy Was Hurt Between Posts Farther Apart Than The Wire's Max Length", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShipCrossesSafely()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        Vector2 center = GetWorldBounds().center;
        yield return PlaceHalalit(center + Vector2.down * HALALIT_DISTANCE_BELOW_POSTS);
        yield return PlantTwoPosts(center, POSTS_DISTANCE);
        var halalitHealth = TestUtils.GetHalalit().GetComponent<Health>();
        var healthBeforeCrossing = halalitHealth.GetHealth();

        // WHEN
        yield return PlaceHalalit(GetWireMiddle());
        var isWireOnWhileShipOnIt = _secondPost.GetComponent<TripwirePost>().GetWire().IsWireOn();
        yield return PlaceHalalit(GetWireMiddle() + Vector2.up * OFF_WIRE_DISTANCE);

        // THEN
        AssertWrapper.IsTrue(isWireOnWhileShipOnIt, "Test Setup: The Wire Was Off While The Ship Crossed", _currentSeed);
        AssertWrapper.AreEqual(healthBeforeCrossing, halalitHealth.GetHealth(), "Wire Hurt The Ship", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WireIsMelee()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);

        // WHEN
        // THEN
        TestUtils.AssertMeleeAttack(ATTACK_NAME, _currentSeed);
    }

    [UnityTest]
    public IEnumerator FloorHoldsWithStamina()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<TripwirePost>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);

        var weaponAttack = TestUtils.GetWeaponAttack();
        var weaponMovement = TestUtils.GetWeaponMovement();
        var minCooldown = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<AttackBehaviour>().MinCooldown;
        RaiseWeaponStaminaPickup(weaponAttack.GetCooldownInterval() - NEAR_ZERO_COOLDOWN_MULTIPLIER);
        yield return null;

        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(Vector2.right, weaponAttack.GetAttackJoystickEdge());
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        // WHEN
        var shotTimes = new List<float>();
        var holdEndTime = Time.time + HOLD_FIRE_TIME;
        while (Time.time < holdEndTime)
        {
            if (weaponAttack.HumbleFixedUpdate(touchOnJoystick))
            {
                shotTimes.Add(Time.time);
            }
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.Greater(minCooldown, 0, "Tripwire Prefab Has No Min Cooldown (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.Less(TestUtils.DEFAULT_ATTACK_STATS_2.Rate * weaponAttack.GetCooldownInterval(), minCooldown, "Test Setup: Stamina Did Not Bring The Fire Rate Under The Floor", _currentSeed);
        AssertWrapper.Greater(shotTimes.Count, 1, "Fewer Than Two Posts Were Shot While Holding Fire", _currentSeed);
        for (int i = 1; i < shotTimes.Count; i++)
        {
            AssertWrapper.GreaterOrEqual(shotTimes[i] - shotTimes[i - 1], minCooldown, "Two Posts Were Shot Closer Than The Minimum Cooldown", _currentSeed, COOLDOWN_DELTA);
        }
    }

    private Bounds GetWorldBounds()
    {
        return TestUtils.GetInternalWorldBoxCollider2D().bounds;
    }

    private IEnumerator PlaceHalalit(Vector2 position)
    {
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), position);
        yield return new WaitForFixedUpdate();
        yield return null;
    }

    // Both posts fly up the same distance, so the wire runs left to right above the center.
    private IEnumerator PlantTwoPosts(Vector2 center, float distance)
    {
        _firstPost = TestUtils.LoadTripwirePost(center + Vector2.left * distance / 2, Vector2.up);
        yield return null;
        _secondPost = TestUtils.LoadTripwirePost(center + Vector2.right * distance / 2, Vector2.up);
        yield return WaitForPlant(_secondPost.GetComponent<TripwirePost>());
    }

    private IEnumerator WaitForPlant(TripwirePost post)
    {
        yield return new WaitForSeconds(post.GetFlightTime() + PLANT_MARGIN);
        yield return new WaitForFixedUpdate();
    }

    private Vector2 GetWireMiddle()
    {
        return (_firstPost.transform.position + _secondPost.transform.position) / 2;
    }

    private DescreteTriggerHarmer GetWireHarmer()
    {
        return _secondPost.GetComponentInChildren<DescreteTriggerHarmer>();
    }

    private void RaiseWeaponStaminaPickup(float cooldownMultiplierSubtraction)
    {
        var properties = new Dictionary<EventProperty, float>
        {
            { EventProperty.WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION, cooldownMultiplierSubtraction }
        };
        ItemEvent.Invoke(EventName.PLAYER_UPGRADE_PICKUP, this, new ItemEventArguments(ItemName.WEAPON_STAMINA, properties));
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
