using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MimicTests
{
    private const int MIMIC_ONE_IN_CHANCE = 6;
    private const int ROLLS = 600;
    private const float ROLLS_DELTA = 35f;
    private const int HIGH_HEALTH = 100;
    private const int HALALIT_HEALTH = 1000;
    private const float HIDDEN_WATCH_TIME = 3f;
    private const float MAX_HIDDEN_DRIFT = 0.3f;
    // Health draws a dynamic health bar 1 second after its Start.
    private const float HEALTH_BAR_DELAY = 1f;
    private const float WAIT_MARGIN = 0.3f;
    private const float MIN_APPROACH = 0.3f;
    private const float APPROACH_TIMEOUT = 1f;
    private const float CLAW_TIMEOUT = 10f;
    private const float COIN_THIEF_WATCH_TIME = 3f;
    private static readonly Vector2 FAR_OFFSET_FROM_HALALIT = new(3, 0);
    private static readonly Vector2 CLOSE_OFFSET_FROM_HALALIT = new(1.2f, 0);
    private static readonly Vector2 SHOT_OFFSET_FROM_MIMIC = new(-2.5f, 0);
    // 6 units apart (loot radius is 8), each more than the wake radius from the Halalit.
    private static readonly Vector2 COIN_THIEF_OFFSET_FROM_HALALIT = new(-3, 3);
    private static readonly Vector2 MIMIC_NEAR_THIEF_OFFSET_FROM_HALALIT = new(3, 3);
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator MimicIsWired()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();

        // WHEN
        var itemDropper = TestUtils.GetItemDropper();
        var dropsBank = TestUtils.GetDropsBank();

        // THEN
        if (itemDropper.GetMimicPrefab() == null)
        {
            AssertWrapper.Fail("ItemDropper has no Mimic Prefab (Needs Ofir in Unity)", _currentSeed);
        }
        AssertWrapper.AreEqual(MIMIC_ONE_IN_CHANCE, itemDropper.GetMimicOneInChance(), "ItemDropper's Mimic One In Chance is not 6 (Needs Ofir in Unity)", _currentSeed);
        if (!dropsBank.GetDefaultDropsList().Exists(dropperDrops => dropperDrops.Key == Dropper.MIMIC))
        {
            AssertWrapper.Fail("DropsBank has no MIMIC row (Needs Ofir in Unity)", _currentSeed);
        }
    }

    [UnityTest]
    public IEnumerator StaysHiddenWhileShipFar()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var mimic = TestUtils.LoadMimic(halalitPosition + FAR_OFFSET_FROM_HALALIT, _currentSeed);
        var disguisedWait = mimic.GetComponent<WaitAttackDisguisedWait>();
        var startPosition = (Vector2)mimic.transform.position;

        // WHEN
        yield return new WaitForSeconds(HIDDEN_WATCH_TIME);

        // THEN
        AssertWrapper.IsTrue(disguisedWait.IsHidden(), "Mimic woke while the Halalit was far", _currentSeed);
        AssertWrapper.IsFalse(mimic.GetComponent<Health>().IsHealthBarVisible(), "Hidden Mimic shows its health bar", _currentSeed);
        AssertWrapper.Less(Vector2.Distance(startPosition, mimic.transform.position), MAX_HIDDEN_DRIFT, "Hidden Mimic moved", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WakesWhenShipClose()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        Vector2 halalitPosition = halalit.transform.position;
        var mimic = TestUtils.LoadMimic(halalitPosition + CLOSE_OFFSET_FROM_HALALIT, _currentSeed);
        var disguisedWait = mimic.GetComponent<WaitAttackDisguisedWait>();
        AssertWrapper.Less(CLOSE_OFFSET_FROM_HALALIT.magnitude, disguisedWait.GetWakeRadius(), "Close offset is outside the wake radius", _currentSeed);
        yield return null;
        halalit.GetComponent<Health>().SetHealth(HALALIT_HEALTH);
        var startTime = Time.time;
        var startDistance = Vector2.Distance(halalit.transform.position, mimic.transform.position);

        // WHEN
        yield return new WaitForSeconds(disguisedWait.GetWakeDelay());
        var approachTimeout = Time.time + APPROACH_TIMEOUT;
        while (Time.time < approachTimeout && Vector2.Distance(halalit.transform.position, mimic.transform.position) > startDistance - MIN_APPROACH)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.IsFalse(disguisedWait.IsHidden(), "Mimic did not wake with the Halalit inside its wake radius", _currentSeed);
        AssertWrapper.GreaterOrEqual(startDistance - MIN_APPROACH, Vector2.Distance(halalit.transform.position, mimic.transform.position), "Awake Mimic did not charge at the Halalit", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(Mathf.Max(0, startTime + HEALTH_BAR_DELAY + WAIT_MARGIN - Time.time));

        // THEN
        AssertWrapper.IsTrue(mimic.GetComponent<Health>().IsHealthBarVisible(), "Awake Mimic does not show its health bar", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WakesWhenShot()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var mimic = TestUtils.LoadMimic(halalitPosition + TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        var disguisedWait = mimic.GetComponent<WaitAttackDisguisedWait>();
        yield return null;
        TestUtils.SetEnemiesHealth(HIGH_HEALTH);

        // WHEN
        TestUtils.LoadBallShot((Vector2)mimic.transform.position + SHOT_OFFSET_FROM_MIMIC, Vector2.right);
        yield return new WaitForSeconds(HEALTH_BAR_DELAY + WAIT_MARGIN);

        // THEN
        AssertWrapper.Less(mimic.GetComponent<Health>().GetHealth(), HIGH_HEALTH, "Ball Shot did not hurt the Mimic", _currentSeed);
        AssertWrapper.IsFalse(disguisedWait.IsHidden(), "Mimic did not wake when shot", _currentSeed);
        AssertWrapper.IsTrue(mimic.GetComponent<Health>().IsHealthBarVisible(), "Shot Mimic does not show its health bar", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ClawCarriesItThenItBites()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var halalit = TestUtils.GetHalalit();
        Vector2 halalitPosition = halalit.transform.position;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        var mimic = TestUtils.LoadMimic(halalitPosition + new Vector2(maneuverRadius - 1, 0), _currentSeed);
        var disguisedWait = mimic.GetComponent<WaitAttackDisguisedWait>();
        AssertWrapper.Greater(maneuverRadius - 1, disguisedWait.GetWakeRadius(), "Claw range is too short for this test", _currentSeed);
        yield return null;
        halalit.GetComponent<Health>().SetHealth(HALALIT_HEALTH);
        var pickupClawShooter = TestUtils.GetPickupClawShooter();

        // WHEN
        pickupClawShooter.TryShootClaw(mimic.transform.position);
        yield return null;
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, _currentSeed, "Claw was not shot at the hidden Mimic");

        var wasCarried = false;
        var timeout = Time.time + CLAW_TIMEOUT;
        while (pickupClaw != null && mimic != null && Time.time < timeout)
        {
            wasCarried |= mimic.transform.parent == pickupClaw.transform;
            yield return null;
        }

        // THEN
        AssertWrapper.IsTrue(mimic != null, "Mimic was destroyed while the claw carried it", _currentSeed);
        AssertWrapper.IsTrue(pickupClaw == null, "Claw did not finish", _currentSeed);
        AssertWrapper.IsTrue(wasCarried, "Claw did not carry the hidden Mimic", _currentSeed);
        AssertWrapper.IsTrue(mimic.transform.parent == null, "Mimic is still held after the claw finished", _currentSeed);
        AssertWrapper.IsFalse(disguisedWait.IsHidden(), "Mimic did not wake when carried to the Halalit", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CoinThiefIgnoresMimic()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var coinThief = TestUtils.LoadCoinThief(halalitPosition + COIN_THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var mimic = TestUtils.LoadMimic(halalitPosition + MIMIC_NEAR_THIEF_OFFSET_FROM_HALALIT, _currentSeed);
        var waitingInterval = coinThief.GetComponent<WaitAttackLootWait>().GetWaitingInterval();

        // WHEN
        yield return new WaitForSeconds(waitingInterval + COIN_THIEF_WATCH_TIME);

        // THEN
        AssertWrapper.IsTrue(mimic != null, "Mimic was destroyed near the Coin Thief", _currentSeed);
        AssertWrapper.IsTrue(mimic.transform.parent == null, "Coin Thief took the Mimic", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DeathDropsWeaponItem()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var mimic = TestUtils.LoadMimic(halalitPosition + TestUtils.DEFAULT_POSITION_TO_THE_RIGHT, _currentSeed);
        yield return null;
        AssertWrapper.Greater(TestUtils.GetDropsBank().GetDrops(Dropper.MIMIC).Count, 0, "DropsBank has no MIMIC drops in stock (Needs Ofir in Unity)", _currentSeed);
        var attackItemsBefore = Object.FindObjectsOfType<AttackItem>().Length;
        var health = mimic.GetComponent<Health>();

        // WHEN
        health.TryChangeHealth(-health.GetHealth());
        yield return null;

        // THEN
        AssertWrapper.IsTrue(mimic == null, "Mimic did not die", _currentSeed);
        AssertWrapper.AreEqual(attackItemsBefore + 1, Object.FindObjectsOfType<AttackItem>().Length, "Dead Mimic did not drop exactly one weapon item", _currentSeed);
    }

    [UnityTest]
    public IEnumerator RollForMimicIsOneInSix()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var itemDropper = TestUtils.GetItemDropper();
        var itemsBank = TestUtils.GetItemsBank();
        if (itemDropper.GetMimicPrefab() == null)
        {
            AssertWrapper.Fail("ItemDropper has no Mimic Prefab (Needs Ofir in Unity)", _currentSeed);
        }
        AssertWrapper.IsTrue(itemsBank.IsAttackItem(ItemName.BALL_SHOT), "Ball Shot is not an attack item in ItemsBank", _currentSeed);
        AssertWrapper.IsFalse(itemsBank.IsAttackItem(ItemName.HALALIT_VITALITY), "Halalit Vitality is an attack item in ItemsBank", _currentSeed);

        // WHEN
        var attackItemMimics = CountMimicRolls(itemDropper, Dropper.ASTEROID, ItemName.BALL_SHOT);
        var upgradeMimics = CountMimicRolls(itemDropper, Dropper.ASTEROID, ItemName.HALALIT_VITALITY);
        var mimicDropperMimics = CountMimicRolls(itemDropper, Dropper.MIMIC, ItemName.BALL_SHOT);

        // THEN
        AssertWrapper.AreEqual(ROLLS / itemDropper.GetMimicOneInChance(), attackItemMimics, "Attack item drops are not a Mimic one time in six", _currentSeed, ROLLS_DELTA);
        AssertWrapper.AreEqual(0, upgradeMimics, "An upgrade drop became a Mimic", _currentSeed);
        AssertWrapper.AreEqual(0, mimicDropperMimics, "A dying Mimic dropped a Mimic", _currentSeed);
    }

    private static int CountMimicRolls(ItemDropper itemDropper, Dropper dropper, ItemName itemName)
    {
        var mimics = 0;
        for (int i = 0; i < ROLLS; i++)
        {
            if (itemDropper.RollForMimic(dropper, itemName))
            {
                mimics++;
            }
        }
        return mimics;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
