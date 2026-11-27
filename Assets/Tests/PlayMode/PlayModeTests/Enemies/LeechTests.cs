using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class LeechTests
{
    private const int HALALIT_HEALTH = 1000;
    private const float FAKE_THRUST_FACTOR = 0.7f;
    private const float FORCE_DELTA = 0.001f;
    private const float LATCH_TIMEOUT = 5f;
    private const float TOUCH_TIMEOUT = 1f;
    private const float RELEASE_TIMEOUT = 1f;
    private const float WAIT_MARGIN = 0.3f;
    // Above the shake-off speed and below the global speed limit (20 in Playground).
    private const float SHAKE_OFF_EXTRA_SPEED = 2f;
    // Covers the gap to the Halalit well within TOUCH_TIMEOUT.
    private const float DISABLED_LEECH_PUSH_SPEED = 5f;
    private static readonly Vector2 RIGHT_OF_HALALIT = new(1.5f, 0);
    private static readonly Vector2 LEFT_OF_HALALIT = new(-1.5f, 0);
    // Close enough that the third Leech touches the Halalit long before the latched ones harm it again.
    private static readonly Vector2 ABOVE_HALALIT = new(0, 1.2f);
    // Puts the Halalit's body over the stunned Leech, so the two touch at once.
    private static readonly Vector2 OVERLAP_OFFSET = new(0.3f, 0);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator LeechIsWired()
    {
        // GIVEN
        yield return null;

        // WHEN
        var leechPrefab = Resources.Load<GameObject>(TestUtils.LEECH_PREFAB_PATH);

        // THEN
        AssertWrapper.IsNotNull(leechPrefab, _currentSeed, $"Leech prefab not found at Resources/{TestUtils.LEECH_PREFAB_PATH} (Needs Ofir in Unity)");
        var collisionHarmers = leechPrefab.GetComponentsInChildren<CollisionHarmer>(true);
        AssertWrapper.AreEqual(1, collisionHarmers.Length, "Leech prefab must have exactly one CollisionHarmer (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.IsTrue(collisionHarmers[0] is LatchCollisionHarmer, "Leech prefab's CollisionHarmer is not a LatchCollisionHarmer (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.IsNotNull(leechPrefab.GetComponent<LeechEnemy>(), _currentSeed, "Leech prefab has no LeechEnemy (Needs Ofir in Unity)");
        AssertWrapper.IsNull(leechPrefab.GetComponent<LameEnemy>(), _currentSeed, "Leech prefab still has LameEnemy (Needs Ofir in Unity)");
        var enemyDropper = leechPrefab.GetComponent<EnemyDropper>();
        AssertWrapper.IsTrue(enemyDropper != null && enemyDropper.GetDropper() == Dropper.LEECH, "Leech prefab's EnemyDropper is not set to LEECH (Needs Ofir in Unity)", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LatchSlowsThrust()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalitMovement = TestUtils.GetHalalitMovement();
        var fakeLeech = new GameObject("FakeLeech");
        AssertWrapper.AreEqual(1f, ShipLatches.GetThrustFactor(), "Thrust is changed with nothing latched", _currentSeed);
        var freeForce = TestUtils.PushHalalitOnce(halalitMovement);
        AssertWrapper.Greater(freeForce.magnitude, 0, "Test setup: the Halalit pushed with no force", _currentSeed);

        // WHEN
        AssertWrapper.IsTrue(ShipLatches.TryLatch(fakeLeech.transform, FAKE_THRUST_FACTOR), "A free slot refused the latch", _currentSeed);
        var latchedForce = TestUtils.PushHalalitOnce(halalitMovement);

        // THEN
        AssertWrapper.AreEqual(freeForce * FAKE_THRUST_FACTOR, latchedForce, "One latch did not scale the thrust by its factor", _currentSeed, FORCE_DELTA);
    }

    [UnityTest]
    public IEnumerator LatchesWithoutTouchDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var leech = TestUtils.LoadLeech((Vector2)halalit.transform.position + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        halalit.GetComponent<Health>().SetHealth(HALALIT_HEALTH);

        // WHEN
        yield return WaitUntilLatched(leech);

        // THEN
        AssertWrapper.IsTrue(leech.IsLatched(), "Leech did not latch onto the Halalit", _currentSeed);
        AssertWrapper.AreEqual(HALALIT_HEALTH, halalit.GetComponent<Health>().GetHealth(), "Latching did touch damage", _currentSeed);
        AssertWrapper.AreEqual(1, ShipLatches.GetLatchedCount(), "Latch did not take a slot", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LatchedLeechHarmsEveryInterval()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        var leech = TestUtils.LoadLeech((Vector2)halalit.transform.position + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        halalitHealth.SetHealth(HALALIT_HEALTH);
        yield return WaitUntilLatched(leech);
        AssertWrapper.IsTrue(leech.IsLatched(), "Leech did not latch onto the Halalit", _currentSeed);
        var healthAtLatch = halalitHealth.GetHealth();

        // WHEN
        yield return new WaitForSeconds(leech.GetHarmInterval() - WAIT_MARGIN);

        // THEN
        AssertWrapper.AreEqual(healthAtLatch, halalitHealth.GetHealth(), "Latched Leech harmed before its interval", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(2 * WAIT_MARGIN);

        // THEN
        AssertWrapper.AreEqual(healthAtLatch - leech.GetLatchedHarm(), halalitHealth.GetHealth(), "Latched Leech did not harm once after its interval", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ThirdLeechBouncesWithDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        Vector2 halalitPosition = halalit.transform.position;
        var rightLeech = TestUtils.LoadLeech(halalitPosition + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        var leftLeech = TestUtils.LoadLeech(halalitPosition + LEFT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        halalitHealth.SetHealth(HALALIT_HEALTH);
        yield return WaitUntilLatched(rightLeech);
        yield return WaitUntilLatched(leftLeech);
        AssertWrapper.IsTrue(rightLeech.IsLatched() && leftLeech.IsLatched(), "Test setup: two Leeches did not latch", _currentSeed);
        AssertWrapper.IsFalse(ShipLatches.HasFreeSlot(), "Two latched Leeches left a free slot", _currentSeed);

        // WHEN
        var thirdLeech = TestUtils.LoadLeech((Vector2)halalit.transform.position + ABOVE_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        var touchHarm = thirdLeech.GetComponent<CollisionHarmer>().GetCollisionHarm();
        AssertWrapper.Greater(touchHarm, 0, "Test setup: a Leech that cannot latch has no touch damage", _currentSeed);
        var healthBeforeTouch = halalitHealth.GetHealth();
        var touchTimeout = Time.time + TOUCH_TIMEOUT;
        while (halalitHealth.GetHealth() == healthBeforeTouch && Time.time < touchTimeout)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsFalse(thirdLeech.IsLatched(), "A third Leech latched", _currentSeed);
        AssertWrapper.AreEqual(healthBeforeTouch - touchHarm, halalitHealth.GetHealth(), "A third Leech did not do its touch damage", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShakeOffReleasesAndStuns()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var halalitRigidbody = halalit.GetComponent<Rigidbody2D>();
        var leech = TestUtils.LoadLeech((Vector2)halalit.transform.position + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        halalit.GetComponent<Health>().SetHealth(HALALIT_HEALTH);
        yield return WaitUntilLatched(leech);
        AssertWrapper.IsTrue(leech.IsLatched(), "Test setup: Leech did not latch", _currentSeed);

        // WHEN
        yield return new WaitForFixedUpdate();
        halalitRigidbody.velocity = Vector2.up * (leech.GetShakeOffSpeed() + SHAKE_OFF_EXTRA_SPEED);
        var releaseTimeout = Time.time + RELEASE_TIMEOUT;
        while (!leech.IsStunned() && Time.time < releaseTimeout)
        {
            yield return new WaitForFixedUpdate();
        }
        var releaseTime = Time.time;

        // THEN
        AssertWrapper.IsTrue(leech.IsStunned(), "Flying faster than the shake-off speed did not stun the Leech", _currentSeed);
        AssertWrapper.AreEqual(0, ShipLatches.GetLatchedCount(), "Shaken-off Leech kept its slot", _currentSeed);
        AssertWrapper.AreEqual(1f, ShipLatches.GetThrustFactor(), "Shaken-off Leech still slows the thrust", _currentSeed);

        // WHEN
        halalitRigidbody.velocity = Vector2.zero;
        halalitRigidbody.position = (Vector2)leech.transform.position + OVERLAP_OFFSET;
        yield return new WaitForSeconds(Mathf.Max(0, releaseTime + leech.GetStunDuration() - WAIT_MARGIN - Time.time));

        // THEN
        AssertWrapper.IsFalse(leech.IsLatched(), "Stunned Leech latched again", _currentSeed);
        AssertWrapper.IsTrue(leech.IsStunned(), "Leech stun ended early", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(2 * WAIT_MARGIN);

        // THEN
        AssertWrapper.IsFalse(leech.IsStunned(), "Leech stun did not end after its duration", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DisabledLeechFreesSlot()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var leech = TestUtils.LoadLeech((Vector2)halalit.transform.position + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        yield return null;
        halalit.GetComponent<Health>().SetHealth(HALALIT_HEALTH);
        yield return WaitUntilLatched(leech);
        AssertWrapper.IsTrue(leech.IsLatched(), "Test setup: Leech did not latch", _currentSeed);

        // WHEN
        leech.enabled = false;
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.IsFalse(leech.IsLatched(), "Disabled Leech stayed latched", _currentSeed);
        AssertWrapper.AreEqual(0, ShipLatches.GetLatchedCount(), "Disabled Leech kept its slot", _currentSeed);
        AssertWrapper.AreEqual(1f, ShipLatches.GetThrustFactor(), "Disabled Leech still slows the thrust", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DisabledLeechDoesNotLatch()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        var leech = TestUtils.LoadLeech((Vector2)halalit.transform.position + RIGHT_OF_HALALIT, _currentSeed).GetComponent<LeechEnemy>();
        leech.enabled = false;
        yield return null;
        halalitHealth.SetHealth(HALALIT_HEALTH);
        var touchHarm = leech.GetComponent<CollisionHarmer>().GetCollisionHarm();
        AssertWrapper.Greater(touchHarm, 0, "Test setup: a disabled Leech has no touch damage", _currentSeed);
        var healthBeforeTouch = halalitHealth.GetHealth();

        // WHEN
        // A disabled Leech does not drift, so push it into the Halalit.
        var leechRigidbody = leech.GetComponent<Rigidbody2D>();
        leechRigidbody.velocity = ((Vector2)halalit.transform.position - leechRigidbody.position).normalized * DISABLED_LEECH_PUSH_SPEED;
        var touchTimeout = Time.time + TOUCH_TIMEOUT;
        while (halalitHealth.GetHealth() == healthBeforeTouch && Time.time < touchTimeout)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsFalse(leech.IsLatched(), "A disabled Leech latched", _currentSeed);
        AssertWrapper.AreEqual(0, ShipLatches.GetLatchedCount(), "A disabled Leech took a slot", _currentSeed);
        AssertWrapper.AreEqual(healthBeforeTouch - touchHarm, halalitHealth.GetHealth(), "A disabled Leech did not do its touch damage", _currentSeed);
    }

    private IEnumerator WaitUntilLatched(LeechEnemy leech)
    {
        var latchTimeout = Time.time + LATCH_TIMEOUT;
        while (!leech.IsLatched() && Time.time < latchTimeout)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    [TearDown]
    public void TearDown()
    {
        ShipLatches.ClearAll();
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
