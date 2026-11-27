using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MenderTests
{
    private const int FULL_HEALTH = 10;
    private const int MISSING_HEALTH = 5;
    private const int SMALL_MISSING_HEALTH = 2;
    private const int ONE_MISSING_HEALTH = 1;
    // Past the first heal, which lands on the first search (every 0.25 s) after the heal interval, and well before a second heal.
    private const float HEAL_MARGIN = 0.5f;
    private const float FOLLOW_START_DISTANCE = 6f;
    // At its speed limit (1.5) the Mender covers the gap in about 2 s, then settles.
    private const float FOLLOW_TIME = 4f;
    // The follow tolerance (0.5) plus room to coast to a stop.
    private const float FOLLOW_MARGIN = 1f;
    private const float MENDERS_DISTANCE = 2f;
    // Keeps the Halalit clear of every Mender and enemy in these tests, a drifting Mender included.
    private const float HALALIT_DISTANCE_BELOW = 6f;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator HealsMostHurtInRange()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        // Both at the follow distance, so the Mender holds still. The less hurt one is on the left, where a tie would go.
        var mostHurt = LoadPatient(anchor + Vector2.right * mender.GetFollowDistance());
        var lessHurt = LoadPatient(anchor + Vector2.left * mender.GetFollowDistance());
        yield return null;
        Hurt(mostHurt, MISSING_HEALTH);
        Hurt(lessHurt, SMALL_MISSING_HEALTH);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(FULL_HEALTH - MISSING_HEALTH + mender.GetHealAmount(), GetHealth(mostHurt), "The Mender did not heal the most hurt enemy by its heal amount", _currentSeed);
        AssertWrapper.AreEqual(FULL_HEALTH - SMALL_MISSING_HEALTH, GetHealth(lessHurt), "The Mender healed an enemy that was not the most hurt", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NeverHealsAboveMax()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        var patient = LoadPatient(anchor + Vector2.right * mender.GetFollowDistance());
        yield return null;
        Hurt(patient, ONE_MISSING_HEALTH);
        AssertWrapper.Greater(mender.GetHealAmount(), ONE_MISSING_HEALTH, "Test setup: the heal amount is not more than the missing health", _currentSeed);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(FULL_HEALTH, GetHealth(patient), "The Mender did not heal the enemy to exactly its max health", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AsteroidBlocksHeal()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        var patientPosition = anchor + Vector2.right * mender.GetFollowDistance();
        var patient = LoadPatient(patientPosition);
        // The loader stops the asteroid. Halfway between the two, it is on the beam's line and clear of both bodies.
        var asteroid = GetObjectLoader().LoadAsteroidInExternalSafeIsland(_currentSeed);
        TestUtils.SetGameObjectPosition(asteroid, (anchor + patientPosition) / 2f);
        yield return null;
        Hurt(patient, MISSING_HEALTH);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(FULL_HEALTH - MISSING_HEALTH, GetHealth(patient), "The Mender healed through an asteroid", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LoneMenderStaysHurt()
    {
        // GIVEN
        yield return SetUpScene();
        var mender = LoadSwitchedOffMender(GetAnchor());
        yield return null;
        Hurt(mender.gameObject, MISSING_HEALTH);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(FULL_HEALTH - MISSING_HEALTH, GetHealth(mender.gameObject), "A lone Mender healed itself", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TwoMendersHealEachOther()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var leftMender = LoadSwitchedOffMender(anchor);
        var rightMender = LoadSwitchedOffMender(anchor + Vector2.right * MENDERS_DISTANCE);
        yield return null;
        Hurt(leftMender.gameObject, MISSING_HEALTH);
        Hurt(rightMender.gameObject, MISSING_HEALTH);

        // WHEN
        yield return StartMenders(leftMender, rightMender);
        yield return new WaitForSeconds(leftMender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        var healedHealth = FULL_HEALTH - MISSING_HEALTH + leftMender.GetHealAmount();
        AssertWrapper.AreEqual(healedHealth, GetHealth(leftMender.gameObject), "The right Mender did not heal the left one", _currentSeed);
        AssertWrapper.AreEqual(healedHealth, GetHealth(rightMender.gameObject), "The left Mender did not heal the right one", _currentSeed);
    }

    [UnityTest]
    public IEnumerator HealDoesNotEndRetreat()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        var patient = LoadPatient(anchor + Vector2.right * mender.GetFollowDistance());
        var woundedRetreat = TestUtils.GetWoundedRetreat(patient);
        yield return null;
        AssertWrapper.Greater(woundedRetreat.GetRetreatDuration(), mender.GetHealInterval() + HEAL_MARGIN, "Test setup: the retreat would end before the check", _currentSeed);
        // Pinned in place, so the test checks a heal during the retreat, not whether the Mender keeps up with a fleeing enemy.
        patient.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePosition;
        Hurt(patient, FULL_HEALTH - woundedRetreat.GetWoundedHealth());
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "Test setup: the wound did not start the retreat", _currentSeed);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(woundedRetreat.GetWoundedHealth() + mender.GetHealAmount(), GetHealth(patient), "The Mender did not heal the retreating enemy by its heal amount", _currentSeed);
        AssertWrapper.IsTrue(woundedRetreat.IsRetreating(), "The heal ended the retreat", _currentSeed);
    }

    [UnityTest]
    public IEnumerator SwitchedOffMenderHidesBeamAndStopsHealing()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        var patient = LoadPatient(anchor + Vector2.right * mender.GetFollowDistance());
        yield return null;
        Hurt(patient, MISSING_HEALTH);
        yield return StartMenders(mender);
        yield return WaitForHealthChange(patient, FULL_HEALTH - MISSING_HEALTH, mender.GetHealInterval() + HEAL_MARGIN);
        var healthAfterHeal = GetHealth(patient);
        AssertWrapper.Greater(healthAfterHeal, FULL_HEALTH - MISSING_HEALTH, "Test setup: the Mender did not heal before it was switched off", _currentSeed);
        AssertWrapper.IsTrue(mender.IsBeamShown(), "The beam did not show with the heal", _currentSeed);

        // WHEN
        mender.enabled = false;

        // THEN
        AssertWrapper.IsFalse(mender.IsBeamShown(), "Switching the Mender off left its beam on", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(healthAfterHeal, GetHealth(patient), "A switched-off Mender healed", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TieGoesToTheLeft()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        // Both at the follow distance, so the Mender holds still and the two distances stay exactly equal.
        var right = LoadPatient(anchor + Vector2.right * mender.GetFollowDistance());
        var left = LoadPatient(anchor + Vector2.left * mender.GetFollowDistance());
        yield return null;
        Hurt(right, MISSING_HEALTH);
        Hurt(left, MISSING_HEALTH);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(mender.GetHealInterval() + HEAL_MARGIN);

        // THEN
        AssertWrapper.AreEqual(FULL_HEALTH - MISSING_HEALTH + mender.GetHealAmount(), GetHealth(left), "On a tie the Mender did not heal the enemy on the left", _currentSeed);
        AssertWrapper.AreEqual(FULL_HEALTH - MISSING_HEALTH, GetHealth(right), "On a tie the Mender healed the enemy on the right", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FollowsHurtEnemy()
    {
        // GIVEN
        yield return SetUpScene();
        Vector2 anchor = GetAnchor();
        var mender = LoadSwitchedOffMender(anchor);
        var patient = LoadPatient(anchor + Vector2.right * FOLLOW_START_DISTANCE);
        yield return null;
        Hurt(patient, MISSING_HEALTH);

        // WHEN
        yield return StartMenders(mender);
        yield return new WaitForSeconds(FOLLOW_TIME);

        // THEN
        var distance = Vector2.Distance(mender.transform.position, patient.transform.position);
        AssertWrapper.Less(distance, mender.GetFollowDistance() + FOLLOW_MARGIN, "The Mender did not close in on the hurt enemy", _currentSeed);
        AssertWrapper.Greater(distance, mender.GetFollowDistance() - FOLLOW_MARGIN, "The Mender came too close to the hurt enemy", _currentSeed);
    }

    // Test mode, with the Halalit moved below the spot the tests use.
    private IEnumerator SetUpScene()
    {
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), GetAnchor() + Vector2.down * HALALIT_DISTANCE_BELOW);
        yield return new WaitForFixedUpdate();
    }

    // Whole numbers, so that distances along x come out exactly equal on both sides for the tie test.
    private Vector2 GetAnchor()
    {
        Vector2 center = TestUtils.GetInternalWorldBoxCollider2D().bounds.center;
        return new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
    }

    // The Mender, switched off until its patients are placed and hurt (see StartMenders).
    private MenderEnemy LoadSwitchedOffMender(Vector2 position)
    {
        var mender = TestUtils.LoadMender(position, _currentSeed).GetComponent<MenderEnemy>();
        mender.enabled = false;
        return mender;
    }

    // A plain enemy from the scene's ObjectLoader (a Lame Enemy, its movement stopped). Out of the ExternalSafeIsland parent,
    // so no parent transform can round its position and break an exact tie.
    private GameObject LoadPatient(Vector2 position)
    {
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        enemy.transform.SetParent(null);
        TestUtils.SetGameObjectPosition(enemy, position);
        return enemy;
    }

    // Sets the max health to FULL_HEALTH, then takes missingHealth away. Health must have run its Start (a frame after loading).
    private void Hurt(GameObject enemy, int missingHealth)
    {
        var health = enemy.GetComponent<Health>();
        health.SetHealth(FULL_HEALTH);
        health.TryChangeHealth(-missingHealth);
    }

    // Switches the Menders on after a physics step has moved everything into place, so their first search already finds the
    // hurt enemies. Then waits a frame for their Start, which times the first heal.
    private IEnumerator StartMenders(params MenderEnemy[] menders)
    {
        yield return new WaitForFixedUpdate();
        yield return null;
        foreach (var mender in menders)
        {
            mender.enabled = true;
        }
        yield return null;
    }

    // Waits, at most the timeout, until the enemy's health is no longer the given value.
    private IEnumerator WaitForHealthChange(GameObject enemy, int health, float timeout)
    {
        var endTime = Time.time + timeout;
        while (GetHealth(enemy) == health && Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private ObjectLoader GetObjectLoader()
    {
        return GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
    }

    private int GetHealth(GameObject enemy)
    {
        return enemy.GetComponent<Health>().GetHealth();
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
