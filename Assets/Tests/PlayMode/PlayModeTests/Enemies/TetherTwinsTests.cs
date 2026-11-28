using Assets.Enums;
using Assets.Models;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Tether Twins (#237). Unless a test says otherwise, the twins are frozen on a level line through the middle of the world,
// TWINS_DISTANCE apart, and the Halalit waits below them, clear of the line.
public class TetherTwinsTests
{
    private const int HALALIT_HEALTH = 1000;
    // One tether hit leaves it above the Wounded Retreat threshold (2), so a hurt enemy does not flee.
    private const int ENEMY_HEALTH = 7;
    private const int TWIN_HEALTH = 10;
    private const int ENEMY_FILLS = 2; // the first fill and one refill (#248)
    private const int CYCLES_TO_WATCH = 2;
    // Inside the spacing (6 to 9), so twins that are not frozen do not push or pull.
    private const float TWINS_DISTANCE = 8f;
    private const float CLOSE_TWINS_DISTANCE = 3f;
    private const float BEYOND_MAX_DISTANCE = 1f;
    private const float HALALIT_DISTANCE_BELOW = 6f;
    private const float OFF_LINE_DISTANCE = 3f;
    private const float ASTEROID_MOVE_DISTANCE = 5f;
    private const float PLAIN_ENEMY_DISTANCE_ABOVE = 4f;
    private const float TURNED_DEGREES = 90f;
    private const float FULL_TURN_DEGREES = 360f;
    private const float TIME_MARGIN = 0.1f;
    private const float FLAIL_CHECK_TIME = 0.25f;
    // More than one physics step of turning (7.2 degrees at one turn a second).
    private const float ANGLE_MARGIN = 10f;
    private const float LENGTH_DELTA = 0.01f;
    private const float SPREAD_TIME = 3f;
    private const float SPACING_MARGIN = 0.5f;
    private const float DRIFT_TIME = 2f;
    private const float MIN_DRIFT_DISTANCE = 1f;
    // A twin cut on purpose, to show that a line can cut a twin at all, must neither die nor flee.
    private const int CUT_TWIN_HEALTH = 1000;
    private const int MIRROR_SHIELD_HEALTH = 100;
    // LoadTetherTwins seeds a pair with seed and seed + 1, so a second pair takes the next two.
    private const int OTHER_PAIR_SEED_OFFSET = 2;
    // Inside the flail's reach (3), with a gap to the survivor's body.
    private const float FLAIL_SWEEP_DISTANCE = 2.5f;
    private const float FLAIL_WATCH_TURNS = 1.25f;
    private const float DECOY_DISTANCE_ABOVE = 6f;
    private const string DECOY_NAME = "TestDecoy";
    private const float SWAP_SHOT_DISTANCE = 4f;
    private const float SWAP_TIMEOUT = 2f;
    private const float SWAP_POSITION_DELTA = 0.5f;
    private const float POSITION_DELTA = 0.01f;
    private const float SPAWN_HOLE_DISTANCE_ABOVE = 3f;
    // Each phase of the scene's spawn hole (opening, open, closing) takes 1 s.
    private const float SPAWN_HOLE_PHASE_TIMEOUT = 2f;
    private const float MIN_SPAWN_POINT_GAP = 0.5f;
    private int _currentSeed;
    private bool _isTimeScaleChanged;
    private GameObject _decoy;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator TetherTwinIsWired()
    {
        // GIVEN
        yield return null;

        // WHEN
        var tetherTwinPrefab = Resources.Load<GameObject>(TestUtils.TETHER_TWIN_PREFAB_PATH);

        // THEN
        AssertWrapper.IsNotNull(tetherTwinPrefab, _currentSeed, $"TetherTwin prefab not found at Resources/{TestUtils.TETHER_TWIN_PREFAB_PATH} (Needs Ofir in Unity, #237)");
        var tetherTwin = tetherTwinPrefab.GetComponent<TetherTwin>();
        AssertWrapper.IsNotNull(tetherTwin, _currentSeed, "TetherTwin prefab has no TetherTwin (Needs Ofir in Unity, #237)");
        AssertWrapper.IsNull(tetherTwinPrefab.GetComponent<LameEnemy>(), _currentSeed, "TetherTwin prefab still has LameEnemy (Needs Ofir in Unity, #237)");
        AssertWrapper.IsNull(tetherTwinPrefab.GetComponent<Interrupt>(), _currentSeed, "TetherTwin prefab has Interrupt: remove it (Needs Ofir in Unity, #237)");
        AssertWrapper.IsNull(tetherTwinPrefab.GetComponent<ShotDodger>(), _currentSeed, "TetherTwin prefab has ShotDodger: remove it (Needs Ofir in Unity, #237)");
        var enemyDropper = tetherTwinPrefab.GetComponent<EnemyDropper>();
        AssertWrapper.IsTrue(enemyDropper != null && enemyDropper.GetDropper() == Dropper.TETHER_TWIN, "TetherTwin prefab's EnemyDropper is not set to TETHER_TWIN (Needs Ofir in Unity, #237)", _currentSeed);

        var tetherPrefab = tetherTwin.GetTetherPrefab();
        AssertWrapper.IsNotNull(tetherPrefab, _currentSeed, "TetherTwin's Tether Prefab field is empty (Needs Ofir in Unity, #237)");
        var edgeCollider = tetherPrefab.GetComponent<EdgeCollider2D>();
        AssertWrapper.IsTrue(edgeCollider != null && edgeCollider.isTrigger, "Tether prefab has no trigger EdgeCollider2D (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.IsNotNull(tetherPrefab.GetComponent<LineRenderer>(), _currentSeed, "Tether prefab has no LineRenderer (Needs Ofir in Unity, #237)");
        var tetherBody = tetherPrefab.GetComponent<Rigidbody2D>();
        AssertWrapper.IsTrue(tetherBody != null && tetherBody.bodyType == RigidbodyType2D.Kinematic && tetherBody.sleepMode == RigidbodySleepMode2D.NeverSleep, "Tether prefab needs a Kinematic Rigidbody2D that never sleeps (Needs Ofir in Unity, #237)", _currentSeed);
        var attackBehaviour = tetherPrefab.GetComponent<AttackBehaviour>();
        AssertWrapper.IsTrue(attackBehaviour != null && attackBehaviour.ShotType == AttackShotType.DESCRETE, "Tether prefab needs a DESCRETE AttackBehaviour (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.Greater(attackBehaviour.AttackStats.Power, 0, "Tether prefab's AttackBehaviour has no Power (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.AreEqual(0, attackBehaviour.AttackStats.Luck, "Tether prefab's AttackBehaviour has Luck: a critical hit would do more than its Power per touch (Needs Ofir in Unity, #237)", _currentSeed);
        var harmer = tetherPrefab.GetComponent<DescreteTriggerHarmer>();
        AssertWrapper.IsNotNull(harmer, _currentSeed, "Tether prefab has no DescreteTriggerHarmer (Needs Ofir in Unity, #237)");
        AssertWrapper.Greater(harmer.GetSameTargetCooldown(), 0, "Tether prefab's DescreteTriggerHarmer has no Same Target Cooldown (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.IsTrue(tetherPrefab.CompareTag(Tag.ENEMY_SHOT.GetDescription()), "Tether prefab is not tagged EnemyShot (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.AreEqual(LayerMask.NameToLayer(Layer.ENEMY_SHOTS.GetDescription()), tetherPrefab.gameObject.layer, "Tether prefab is not on the EnemyShots layer (Needs Ofir in Unity, #237)", _currentSeed);
    }

    [UnityTest]
    public IEnumerator OneEntrySpawnsALinkedPair()
    {
        // GIVEN
        yield return SetUpScene();
        var plainEnemyPrefab = Resources.Load<GameObject>(TestUtils.ZIG_ZAG_ENEMY_PREFAB_PATH);
        AssertWrapper.IsNotNull(plainEnemyPrefab, _currentSeed, $"Test setup: no enemy prefab at Resources/{TestUtils.ZIG_ZAG_ENEMY_PREFAB_PATH}");

        // WHEN
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var plainEnemies = SpawnHole.InstantiateEnemyEntity(new EnemyEntity(plainEnemyPrefab, _currentSeed), GetAnchor() + Vector2.up * PLAIN_ENEMY_DISTANCE_ABOVE, null);

        // THEN
        AssertWrapper.AreEqual(2, twins.Count, "One paired entry did not make two bodies", _currentSeed);
        var firstTwin = twins[0].GetComponent<TetherTwin>();
        var secondTwin = twins[1].GetComponent<TetherTwin>();
        AssertWrapper.IsTrue(firstTwin.GetPartner() == secondTwin && secondTwin.GetPartner() == firstTwin, "The two twins are not each other's partner", _currentSeed);
        AssertWrapper.IsNotNull(firstTwin.GetTether(), _currentSeed, "The pair got no tether");
        AssertWrapper.IsTrue(firstTwin.GetTether() == secondTwin.GetTether(), "The two twins do not share one tether", _currentSeed);
        AssertWrapper.AreEqual(1, Object.FindObjectsOfType<Tether>().Length, "One pair did not make exactly one tether", _currentSeed);
        var firstDraw = twins[0].GetComponent<EnemySharedBehavior>()._seedfulRandomGenerator.GetNumber();
        var secondDraw = twins[1].GetComponent<EnemySharedBehavior>()._seedfulRandomGenerator.GetNumber();
        AssertWrapper.IsTrue(firstDraw != secondDraw, "The two twins drew the same first number: they were not seeded apart", _currentSeed);
        AssertWrapper.AreEqual(1, plainEnemies.Count, "A plain entry did not make exactly one body", _currentSeed);
        AssertWrapper.IsTrue(plainEnemies[0].GetComponent<EnemySharedBehavior>()._seedfulRandomGenerator != null, "A plain entry's body was not seeded", _currentSeed);
    }

    // Reads the scene's own bank, so it fails until the Tether Twin is in it
    [UnityTest]
    public IEnumerator PairedEntriesGetAPartnerSeed()
    {
        // GIVEN
        yield return null;
        var enemyBank = TestUtils.GetEnemyBank();
        var enemiesPerFill = enemyBank.GetEnemiesPerFill();

        // WHEN
        var enemies = TestUtils.TakeEnemyGroups(enemyBank, ENEMY_FILLS, _currentSeed).SelectMany(enemyGroup => enemyGroup).ToList();

        // THEN
        for (int fill = 0; fill < ENEMY_FILLS; fill++)
        {
            var pairedEnemies = enemies.Skip(fill * enemiesPerFill).Take(enemiesPerFill).Count(enemy => IsPairedPrefab(enemy.Prefab));
            AssertWrapper.Greater(pairedEnemies, 0, $"Fill {fill + 1} of the scene's enemy bank has no paired enemy: add TetherTwin to the EnemyBank with count 2 (Needs Ofir in Unity, #237)", _currentSeed);
        }
        foreach (var enemy in enemies)
        {
            AssertWrapper.IsTrue(enemy.HasPartner == IsPairedPrefab(enemy.Prefab), $"{enemy.Prefab.name} has HasPartner {enemy.HasPartner}: exactly the entries of a paired prefab have a partner", _currentSeed);
            if (enemy.HasPartner)
            {
                AssertWrapper.IsTrue(enemy.PartnerSeed != enemy.Seed, $"{enemy.Prefab.name}'s partner seed is its own seed: the partner needs a draw of its own", _currentSeed);
            }
        }
    }

    [UnityTest]
    public IEnumerator TetherFollowsItsCycle()
    {
        // GIVEN
        yield return SetUpScene();
        var tether = GetTether(LoadLevelTwins(TWINS_DISTANCE));
        yield return WaitUntilHarmful(tether);
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);
        var onStartTime = Time.time;

        // WHEN
        while (tether.IsHarmful() && Time.time < onStartTime + tether.GetOnDuration() + TIME_MARGIN)
        {
            yield return new WaitForFixedUpdate();
        }
        var onEndTime = Time.time;
        var flickerStartTime = float.MaxValue;
        var wasShownBeforeFlicker = false;
        var wasShownInFlicker = false;
        var wasHiddenInFlicker = false;
        while (!tether.IsHarmful() && Time.time < onEndTime + tether.GetOffDuration() + TIME_MARGIN)
        {
            if (tether.GetState() == TetherState.FLICKERING)
            {
                flickerStartTime = Mathf.Min(flickerStartTime, Time.time);
                wasShownInFlicker |= tether.IsVisible();
                wasHiddenInFlicker |= !tether.IsVisible();
            }
            else
            {
                wasShownBeforeFlicker |= tether.IsVisible();
            }
            yield return new WaitForFixedUpdate();
        }
        var nextOnTime = Time.time;

        // THEN
        AssertWrapper.AreEqual(tether.GetOnDuration(), onEndTime - onStartTime, "The tether was not harmful for its on time", _currentSeed, TIME_MARGIN);
        AssertWrapper.IsTrue(tether.IsHarmful(), "The tether did not come on again after its off time", _currentSeed);
        AssertWrapper.AreEqual(tether.GetOffDuration(), nextOnTime - onEndTime, "The tether was not harmless for its off time", _currentSeed, TIME_MARGIN);
        AssertWrapper.AreEqual(tether.GetFlickerDuration(), nextOnTime - flickerStartTime, "The flicker did not take the last part of the off time", _currentSeed, TIME_MARGIN);
        AssertWrapper.IsFalse(wasShownBeforeFlicker, "The line showed while off, before its flicker", _currentSeed);
        AssertWrapper.IsTrue(wasShownInFlicker && wasHiddenInFlicker, "The line did not blink during its flicker", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TetherHurtsShipOncePerTouch()
    {
        // GIVEN
        yield return SetUpScene();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        halalitHealth.SetHealth(HALALIT_HEALTH);
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var tetherHarm = GetTetherHarm(tether);
        var sameTargetCooldown = tether.GetComponent<DescreteTriggerHarmer>().GetSameTargetCooldown();
        AssertWrapper.Greater(sameTargetCooldown, 0, "Tether's DescreteTriggerHarmer has no Same Target Cooldown (Needs Ofir in Unity, #237)", _currentSeed);
        yield return WaitUntilHarmful(tether);
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);
        var lineMiddle = GetLineMiddle(twins);
        var offLine = lineMiddle + Vector2.down * OFF_LINE_DISTANCE;

        // WHEN
        TestUtils.SetGameObjectPosition(halalit, lineMiddle);
        yield return new WaitForFixedUpdate();
        var firstContactTime = Time.time;
        var healthAfterFirstContact = halalitHealth.GetHealth();

        TestUtils.SetGameObjectPosition(halalit, offLine);
        yield return new WaitForFixedUpdate();
        TestUtils.SetGameObjectPosition(halalit, lineMiddle);
        yield return new WaitForFixedUpdate();
        var quickReturnTime = Time.time;
        var healthAfterQuickReturn = halalitHealth.GetHealth();

        TestUtils.SetGameObjectPosition(halalit, offLine);
        yield return new WaitForFixedUpdate();
        while (Time.time < firstContactTime + sameTargetCooldown + TIME_MARGIN)
        {
            yield return new WaitForFixedUpdate();
        }
        TestUtils.SetGameObjectPosition(halalit, lineMiddle);
        yield return new WaitForFixedUpdate();
        var healthAfterLateReturn = halalitHealth.GetHealth();
        var wasOnAtLateReturn = tether.IsHarmful();

        // THEN
        AssertWrapper.IsTrue(wasOnAtLateReturn, "Test setup: the tether went off before the late return", _currentSeed);
        AssertWrapper.Less(quickReturnTime, firstContactTime + sameTargetCooldown, "Test setup: the quick return came after the same-target cooldown", _currentSeed);
        AssertWrapper.AreEqual(HALALIT_HEALTH - tetherHarm, healthAfterFirstContact, "The tether did not hurt the ship once on contact", _currentSeed);
        AssertWrapper.AreEqual(HALALIT_HEALTH - tetherHarm, healthAfterQuickReturn, "The tether hurt the ship again within its same-target cooldown", _currentSeed);
        AssertWrapper.AreEqual(HALALIT_HEALTH - 2 * tetherHarm, healthAfterLateReturn, "The tether did not hurt the ship again after its same-target cooldown", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TetherHurtsOtherEnemies()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var tetherHarm = GetTetherHarm(tether);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;
        enemy.GetComponent<Health>().SetHealth(ENEMY_HEALTH);
        yield return WaitUntilHarmful(tether);
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);

        // WHEN
        TestUtils.SetGameObjectPosition(enemy, GetLineMiddle(twins));
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - tetherHarm, enemy.GetComponent<Health>().GetHealth(), "The tether did not hurt the enemy on it once (Needs Ofir in Unity: Enemy Shot in its harmers?)", _currentSeed);
    }

    // Turned a quarter, a twin's long body (Lame's capsule) lies along the line, as near to the harm segment as it gets.
    // A twin of a second pair sits on the line, so the test also sees that a line cuts a twin at all.
    [UnityTest]
    public IEnumerator TwinsAreNotHurtByTheirOwnTether()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        foreach (var twin in twins)
        {
            twin.transform.rotation = Quaternion.Euler(0, 0, TURNED_DEGREES);
        }
        var twinOnTheLine = LoadOtherPair(GetLineMiddle(twins), Vector2.up)[0];
        yield return null;
        foreach (var twin in twins)
        {
            twin.GetComponent<Health>().SetHealth(TWIN_HEALTH);
        }
        twinOnTheLine.GetComponent<Health>().SetHealth(CUT_TWIN_HEALTH);

        // WHEN
        var wasHarmful = false;
        var endTime = Time.time + CYCLES_TO_WATCH * (tether.GetOnDuration() + tether.GetOffDuration());
        while (Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
            wasHarmful |= tether.IsHarmful();
        }

        // THEN
        AssertWrapper.IsTrue(wasHarmful, "Test setup: the tether never came on", _currentSeed);
        AssertWrapper.Less(twinOnTheLine.GetComponent<Health>().GetHealth(), CUT_TWIN_HEALTH, "Test setup: a twin of another pair sitting on the line was not cut, so this test cannot see a twin cut by a line (Needs Ofir in Unity: Enemy Shot in the twin's harmers?)", _currentSeed);
        foreach (var twin in twins)
        {
            AssertWrapper.AreEqual(TWIN_HEALTH, twin.GetComponent<Health>().GetHealth(), "A twin was hurt by its own tether", _currentSeed);
        }
    }

    [UnityTest]
    public IEnumerator TetherOffBeyondMaxDistance()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var farDistance = tether.GetMaxDistance() + BEYOND_MAX_DISTANCE;
        TestUtils.SetGameObjectPosition(twins[0], GetAnchor() + Vector2.left * farDistance / 2);
        TestUtils.SetGameObjectPosition(twins[1], GetAnchor() + Vector2.right * farDistance / 2);

        // WHEN
        var wasShown = false;
        var didReachOn = false;
        var endTime = Time.time + tether.GetOffDuration() + tether.GetOnDuration();
        while (Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
            wasShown |= tether.IsVisible() || tether.IsHarmful();
            didReachOn |= tether.GetState() == TetherState.ON;
        }

        // THEN
        AssertWrapper.IsTrue(didReachOn, "Test setup: the tether's cycle never reached ON", _currentSeed);
        AssertWrapper.IsFalse(wasShown, "The tether showed between twins farther apart than its max distance", _currentSeed);

        // WHEN
        // Without this step the test would also pass with a tether that never comes on.
        TestUtils.SetGameObjectPosition(twins[0], GetAnchor() + Vector2.left * TWINS_DISTANCE / 2);
        TestUtils.SetGameObjectPosition(twins[1], GetAnchor() + Vector2.right * TWINS_DISTANCE / 2);
        yield return WaitUntilHarmful(tether);

        // THEN
        AssertWrapper.IsTrue(tether.IsHarmful(), "With the twins back within its max distance the tether still did not come on", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AsteroidBetweenCutsTether()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        // The loader stops the asteroid. On the middle of the line, it is clear of both twins.
        var asteroid = GetObjectLoader().LoadAsteroidInExternalSafeIsland(_currentSeed);
        TestUtils.SetGameObjectPosition(asteroid, GetLineMiddle(twins));

        // WHEN
        var wasShownThroughAsteroid = false;
        var didReachOn = false;
        var endTime = Time.time + tether.GetOffDuration() + tether.GetOnDuration();
        while (Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
            wasShownThroughAsteroid |= tether.IsVisible() || tether.IsHarmful();
            didReachOn |= tether.GetState() == TetherState.ON;
        }

        // THEN
        AssertWrapper.IsTrue(didReachOn, "Test setup: the tether's cycle never reached ON", _currentSeed);
        AssertWrapper.IsFalse(wasShownThroughAsteroid, "The tether showed through an asteroid", _currentSeed);

        // WHEN
        // Without this step the test would also pass with a tether that never comes on.
        TestUtils.SetGameObjectPosition(asteroid, GetLineMiddle(twins) + Vector2.up * ASTEROID_MOVE_DISTANCE);
        yield return WaitUntilHarmful(tether);

        // THEN
        AssertWrapper.IsTrue(tether.IsHarmful(), "With the asteroid moved away the tether still did not come on", _currentSeed);
    }

    // The first twin owns the line, so killing it also checks that the survivor takes the line over
    [UnityTest]
    public IEnumerator SurvivorSwingsFlail()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var survivor = twins[1];
        yield return null;

        // WHEN
        Kill(twins[0]);
        yield return null;
        yield return new WaitForFixedUpdate();
        Vector2 survivorPosition = survivor.transform.position;
        var startDirection = tether.GetHarmEnd() - survivorPosition;
        var startTime = Time.time;
        while (Time.time < startTime + FLAIL_CHECK_TIME)
        {
            yield return new WaitForFixedUpdate();
        }
        var endDirection = tether.GetHarmEnd() - survivorPosition;
        var expectedTurn = FULL_TURN_DEGREES * tether.GetFlailTurnsPerSecond() * (Time.time - startTime);
        var harmStartDistance = Vector2.Distance(survivorPosition, tether.GetHarmStart());

        // THEN
        AssertWrapper.IsNull(twins[0], _currentSeed, "Test setup: the killed twin is still there");
        AssertWrapper.IsTrue(tether.GetState() == TetherState.FLAIL, "The tether did not turn into a flail when one twin died", _currentSeed);
        AssertWrapper.IsTrue(tether.IsHarmful(), "The flail is not harmful", _currentSeed);
        AssertWrapper.Greater(harmStartDistance - tether.GetComponent<EdgeCollider2D>().edgeRadius, survivor.GetComponent<TetherTwin>().GetBodyRadius() + LENGTH_DELTA, "The flail's trigger edge, with its rounded end, reaches the survivor's body", _currentSeed);
        AssertWrapper.AreEqual(tether.GetFlailLength(), endDirection.magnitude, "The flail does not end at its length from the survivor", _currentSeed, LENGTH_DELTA);
        AssertWrapper.AreEqual(expectedTurn, Vector2.Angle(startDirection, endDirection), $"The flail did not turn about {expectedTurn} degrees in {FLAIL_CHECK_TIME} s", _currentSeed, ANGLE_MARGIN);
        AssertWrapper.IsTrue(tether.GetComponent<ShotOwner>().GetOwner() == survivor.transform, "The survivor does not own the flail", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TetherRemovedWhenBothDie()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        // Without this check the test would also pass for a pair that never got a tether.
        AssertWrapper.IsNotNull(tether, _currentSeed, "Test setup: the pair got no tether");
        yield return null;

        // WHEN
        Kill(twins[0]);
        Kill(twins[1]);
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsNull(twins[0], _currentSeed, "Test setup: the first twin is still there");
        AssertWrapper.IsNull(twins[1], _currentSeed, "Test setup: the second twin is still there");
        AssertWrapper.IsNull(tether, _currentSeed, "The tether stayed after both twins died");
    }

    [UnityTest]
    public IEnumerator PausedTwinDropsTether()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var firstTwin = twins[0].GetComponent<TetherTwin>();
        yield return WaitUntilHarmful(tether);
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);

        // WHEN
        firstTwin.enabled = false;

        // THEN
        AssertWrapper.IsFalse(tether.IsHarmful() || tether.IsVisible(), "Switching a twin off did not drop the line at once", _currentSeed);

        // WHEN
        yield return new WaitForFixedUpdate();
        var wasOnWhilePaused = tether.IsHarmful();
        firstTwin.enabled = true;
        yield return WaitUntilHarmful(tether);

        // THEN
        AssertWrapper.IsFalse(wasOnWhilePaused, "The line came back while a twin was switched off", _currentSeed);
        AssertWrapper.IsTrue(tether.IsHarmful(), "The line did not come back after the twin was switched on again", _currentSeed);

        // WHEN
        Kill(twins[1]);
        yield return null;
        yield return new WaitForFixedUpdate();
        AssertWrapper.IsTrue(tether.GetState() == TetherState.FLAIL && tether.IsHarmful(), "Test setup: the survivor's flail is not on", _currentSeed);
        firstTwin.enabled = false;
        var wasFlailOnRightAfterPause = tether.IsHarmful();
        yield return new WaitForFixedUpdate();
        var wasFlailOnWhilePaused = tether.IsHarmful();
        firstTwin.enabled = true;
        yield return new WaitForFixedUpdate();

        // THEN
        AssertWrapper.IsFalse(wasFlailOnRightAfterPause, "Switching the survivor off did not drop its flail at once", _currentSeed);
        AssertWrapper.IsFalse(wasFlailOnWhilePaused, "The flail came back while its survivor was switched off", _currentSeed);
        AssertWrapper.IsTrue(tether.IsHarmful(), "The flail did not come back after its survivor was switched on again", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TwinsSpreadToTheirSpacing()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(CLOSE_TWINS_DISTANCE, frozen: false);
        var firstTwin = twins[0].GetComponent<TetherTwin>();
        AssertWrapper.Less(CLOSE_TWINS_DISTANCE, firstTwin.GetMinSpacing(), "Test setup: the twins do not start closer than their spacing", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(SPREAD_TIME);

        // THEN
        var spacing = Vector2.Distance(twins[0].transform.position, twins[1].transform.position);
        AssertWrapper.GreaterOrEqual(spacing, firstTwin.GetMinSpacing(), $"The twins are {spacing} apart after {SPREAD_TIME} s, closer than their spacing", _currentSeed, SPACING_MARGIN);
        AssertWrapper.Less(spacing, firstTwin.GetMaxSpacing() + SPACING_MARGIN, $"The twins are {spacing} apart after {SPREAD_TIME} s, farther than their spacing", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PairDriftsTowardTarget()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE, frozen: false);
        Vector2 halalitPosition = TestUtils.GetHalalit().transform.position;
        var startDistance = Vector2.Distance(GetLineMiddle(twins), halalitPosition);

        // WHEN
        yield return new WaitForSeconds(DRIFT_TIME);

        // THEN
        var endDistance = Vector2.Distance(GetLineMiddle(twins), halalitPosition);
        AssertWrapper.Less(endDistance, startDistance - MIN_DRIFT_DISTANCE, $"The point between the twins went from {startDistance} to {endDistance} from the ship in {DRIFT_TIME} s: it did not drift toward it", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TetherCutGivesGrudge()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var enemy = GetObjectLoader().LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.GetGrudge(enemy);
        yield return null;
        enemy.GetComponent<Health>().SetHealth(ENEMY_HEALTH);
        yield return WaitUntilHarmful(tether);
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);

        // WHEN
        TestUtils.SetGameObjectPosition(enemy, GetLineMiddle(twins));
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.Greater(ENEMY_HEALTH, enemy.GetComponent<Health>().GetHealth(), "Test setup: the tether did not hurt the enemy (Needs Ofir in Unity: Enemy Shot in its harmers?)", _currentSeed);
        AssertWrapper.IsTrue(tether.GetComponent<ShotOwner>().GetOwner() == twins[0].transform, "The first twin does not own the tether", _currentSeed);
        AssertWrapper.IsTrue(EnemyTarget.HasGrudge(enemy.transform), "The tether's cut started no grudge", _currentSeed);
        AssertWrapper.AreEqual(twins[0].transform.position, EnemyTarget.GetPosition(enemy.transform), "The cut enemy does not aim at the twin that owns the tether", _currentSeed);
    }

    // The ship sits still on the line's path through the off time and the blink: the blink never hurts it.
    // The lit line does, which also shows that the ship was on the line's path all along.
    [UnityTest]
    public IEnumerator BlinkNeverHurtsAShipOnTheLine()
    {
        // GIVEN
        yield return SetUpScene();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        halalitHealth.SetHealth(HALALIT_HEALTH);
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);

        // WHEN
        TestUtils.SetGameObjectPosition(halalit, GetLineMiddle(twins));
        var healthThroughBlink = HALALIT_HEALTH;
        var wasShownInFlicker = false;
        var timeout = Time.time + tether.GetOffDuration() + TIME_MARGIN;
        while (tether.GetState() != TetherState.ON && Time.time < timeout)
        {
            yield return new WaitForFixedUpdate();
            if (tether.GetState() != TetherState.ON)
            {
                healthThroughBlink = halalitHealth.GetHealth();
                wasShownInFlicker |= tether.GetState() == TetherState.FLICKERING && tether.IsVisible();
            }
        }
        var wasOn = tether.GetState() == TetherState.ON;
        yield return new WaitForFixedUpdate();
        var healthOnceOn = halalitHealth.GetHealth();

        // THEN
        AssertWrapper.IsTrue(wasShownInFlicker, "Test setup: the line did not blink while the ship sat on it", _currentSeed);
        AssertWrapper.IsTrue(wasOn, "Test setup: the line never came on", _currentSeed);
        AssertWrapper.AreEqual(HALALIT_HEALTH, healthThroughBlink, "The blinking line hurt the ship", _currentSeed);
        AssertWrapper.Less(healthOnceOn, HALALIT_HEALTH, "Test setup: the lit line did not hurt the ship, so the ship was not on its path", _currentSeed);
    }

    // A ship that sits still on the line's path from before the line comes on, long enough to fall asleep, is hurt when it comes on,
    // then at most once per same-target cooldown while it stays on: the offer's cap. Unity may or may not send a new touch as the
    // line's ends are rewritten each tick; either way the count stays within the cap.
    [UnityTest]
    public IEnumerator StillShipOnALitLineIsHurtAtMostOncePerCooldown()
    {
        // GIVEN
        yield return SetUpScene();
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        halalitHealth.SetHealth(HALALIT_HEALTH);
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var tetherHarm = GetTetherHarm(tether);
        var sameTargetCooldown = tether.GetComponent<DescreteTriggerHarmer>().GetSameTargetCooldown();
        AssertWrapper.Greater(sameTargetCooldown, 0, "Tether's DescreteTriggerHarmer has no Same Target Cooldown (Needs Ofir in Unity, #237)", _currentSeed);
        TestUtils.SetGameObjectPosition(halalit, GetLineMiddle(twins));

        // WHEN
        yield return WaitUntilHarmful(tether);
        var wasOn = tether.IsHarmful();
        var onStartTime = Time.time;
        while (tether.IsHarmful() && Time.time < onStartTime + tether.GetOnDuration() + TIME_MARGIN)
        {
            yield return new WaitForFixedUpdate();
        }
        var healthLost = HALALIT_HEALTH - halalitHealth.GetHealth();

        // THEN
        var maxHits = Mathf.FloorToInt(tether.GetOnDuration() / sameTargetCooldown) + 1;
        AssertWrapper.IsTrue(wasOn, "Test setup: the tether never came on", _currentSeed);
        AssertWrapper.GreaterOrEqual(healthLost, tetherHarm, "The line did not hurt a ship that sat still on its path when it came on (Needs Ofir in Unity: the Tether's body must never sleep)", _currentSeed);
        AssertWrapper.GreaterOrEqual(maxHits * tetherHarm, healthLost, $"A ship that sat on the lit line lost {healthLost} in {tether.GetOnDuration()} s: more than {tetherHarm} per {sameTargetCooldown} s", _currentSeed);
    }

    // The flail turns round its survivor for more than a whole turn, so it passes along the survivor's long body twice, and never cuts it.
    // A twin of a second pair stands in the flail's sweep, so the test also sees that the flail cuts a twin at all.
    [UnityTest]
    public IEnumerator FlailNeverCutsItsSurvivor()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var survivor = twins[1];
        var twinInTheSweep = LoadOtherPair((Vector2)survivor.transform.position + Vector2.up * FLAIL_SWEEP_DISTANCE, Vector2.right)[0];
        yield return null;
        survivor.GetComponent<Health>().SetHealth(TWIN_HEALTH);
        twinInTheSweep.GetComponent<Health>().SetHealth(CUT_TWIN_HEALTH);

        // WHEN
        Kill(twins[0]);
        yield return null;
        var wasFlailHarmful = false;
        var endTime = Time.time + FLAIL_WATCH_TURNS / tether.GetFlailTurnsPerSecond();
        while (Time.time < endTime)
        {
            yield return new WaitForFixedUpdate();
            wasFlailHarmful |= tether.GetState() == TetherState.FLAIL && tether.IsHarmful();
        }

        // THEN
        AssertWrapper.IsTrue(wasFlailHarmful, "Test setup: the flail never came on", _currentSeed);
        AssertWrapper.Less(twinInTheSweep.GetComponent<Health>().GetHealth(), CUT_TWIN_HEALTH, "Test setup: the flail did not cut a twin standing in its sweep, so this test cannot see a twin cut by a flail (Needs Ofir in Unity: Enemy Shot in the twin's harmers?)", _currentSeed);
        AssertWrapper.AreEqual(TWIN_HEALTH, survivor.GetComponent<Health>().GetHealth(), "The flail cut its own survivor", _currentSeed);
    }

    // Mirror Shield (#200): its plate blocks and sends back only Shot. The line is an Enemy Shot, so it hurts a Mirror Shield whose plate
    // faces the tether object (the side its FrontArmor judges a hit from), and the plate neither blocks it nor removes it with a send-back.
    [UnityTest]
    public IEnumerator TetherHurtsMirrorShieldThroughItsPlate()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var tetherHarm = GetTetherHarm(tether);
        var lineMiddle = GetLineMiddle(twins);
        var mirrorShield = TestUtils.LoadMirrorShield(lineMiddle, _currentSeed);
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, (Vector2)tether.transform.position - lineMiddle);
        yield return null;
        mirrorShield.GetComponent<Health>().SetHealth(MIRROR_SHIELD_HEALTH);
        var enemyShotsBefore = GameObject.FindGameObjectsWithTag(Tag.ENEMY_SHOT.GetDescription()).Length;

        // WHEN
        yield return WaitUntilHarmful(tether);
        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(tether, _currentSeed, "The Mirror Shield's plate removed the line");
        AssertWrapper.IsTrue(tether.IsHarmful(), "Test setup: the tether never came on", _currentSeed);
        AssertWrapper.AreEqual(MIRROR_SHIELD_HEALTH - tetherHarm, mirrorShield.GetComponent<Health>().GetHealth(), "The line did not hurt the Mirror Shield once through its plate (Needs Ofir in Unity: Enemy Shot in its harmers?)", _currentSeed);
        AssertWrapper.AreEqual(enemyShotsBefore, GameObject.FindGameObjectsWithTag(Tag.ENEMY_SHOT.GetDescription()).Length, "The Mirror Shield sent a shot back off the line", _currentSeed);
    }

    // Decoy (#163): the pair drifts toward the newest decoy, not the ship, because it asks EnemyTarget. The decoy is on the far side from the ship.
    [UnityTest]
    public IEnumerator PairDriftsTowardDecoy()
    {
        // GIVEN
        yield return SetUpScene();
        var twins = LoadLevelTwins(TWINS_DISTANCE, frozen: false);
        _decoy = new GameObject(DECOY_NAME);
        _decoy.transform.position = GetAnchor() + Vector2.up * DECOY_DISTANCE_ABOVE;
        EnemyTarget.AddDecoy(_decoy.transform);
        Vector2 decoyPosition = _decoy.transform.position;
        var startDistance = Vector2.Distance(GetLineMiddle(twins), decoyPosition);

        // WHEN
        yield return new WaitForSeconds(DRIFT_TIME);

        // THEN
        var endDistance = Vector2.Distance(GetLineMiddle(twins), decoyPosition);
        AssertWrapper.Less(endDistance, startDistance - MIN_DRIFT_DISTANCE, $"The point between the twins went from {startDistance} to {endDistance} from the decoy in {DRIFT_TIME} s: it did not drift toward the decoy", _currentSeed);
    }

    // Swap Shot (#161) swaps the ship with a twin as with any enemy, and that twin's end of the line moves with it
    [UnityTest]
    public IEnumerator SwapShotMovesItsTwinsEndOfTheLine()
    {
        // GIVEN
        yield return SetUpScene();
        TestUtils.AssertAttackPrefabHas<SwapShot>(AttackName.SWAP_SHOT, _currentSeed);
        TestUtils.SetUpShot(AttackName.SWAP_SHOT);
        var twins = LoadLevelTwins(TWINS_DISTANCE);
        var tether = GetTether(twins);
        var halalit = TestUtils.GetHalalit();
        // Straight below the first twin, so the shot flies up into it, clear of the line
        TestUtils.SetGameObjectPosition(halalit, (Vector2)twins[0].transform.position + Vector2.down * SWAP_SHOT_DISTANCE);
        yield return new WaitForFixedUpdate();
        Vector2 halalitStartPosition = halalit.transform.position;
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(Vector2.up, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;
        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        var swapTimeout = Time.time + SWAP_TIMEOUT;
        while (Vector2.Distance(twins[0].transform.position, halalitStartPosition) > SWAP_POSITION_DELTA && Time.time < swapTimeout)
        {
            yield return new WaitForFixedUpdate();
        }
        // One more step, so the line is drawn from where physics has put the twin
        yield return new WaitForFixedUpdate();
        yield return WaitUntilHarmful(tether);

        // THEN
        AssertWrapper.AreEqual(halalitStartPosition, twins[0].transform.position, "Test setup: the Swap Shot did not swap the ship with the twin", _currentSeed, SWAP_POSITION_DELTA);
        AssertWrapper.IsTrue(tether.IsHarmful(), "The line did not come on after the swap", _currentSeed);
        AssertWrapper.AreEqual(GetLineMiddle(twins), (tether.GetHarmStart() + tether.GetHarmEnd()) / 2f, "The line did not move with the swapped twin", _currentSeed, POSITION_DELTA);
    }

    // A spawn hole lets a paired entry out as two linked bodies, each at a spawn point of its own, and switches them on.
    // With one spawn point per entry, as before #237, the partner would throw and the hole would never finish.
    // Reads the scene's bank, so it fails until the Tether Twin is in it. The bank's groups are read on a first load and replayed with
    // the same seed, as SameSeedTests does, so the test knows which group holds a pair before its hole takes it. The scene's spawner
    // is stopped, so only the test's hole takes groups from the bank.
    [UnityTest]
    public IEnumerator SpawnHoleLetsOutALinkedPair()
    {
        // GIVEN
        yield return null;
        var firstFillGroups = TestUtils.TakeEnemyGroups(TestUtils.GetEnemyBank(), 1, _currentSeed);
        var pairGroupIndex = firstFillGroups.FindIndex(enemyGroup => enemyGroup.Any(enemy => enemy.HasPartner));
        AssertWrapper.IsTrue(pairGroupIndex >= 0, "The scene's enemy bank has no paired entry: add TetherTwin to the EnemyBank with count 2 (Needs Ofir in Unity, #237)", _currentSeed);
        var pairGroupEntries = firstFillGroups[pairGroupIndex].Count;
        var pairGroupPairs = firstFillGroups[pairGroupIndex].Count(enemy => enemy.HasPartner);

        TestUtils.SetRandomSeed(_currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        _isTimeScaleChanged = true;
        TestUtils.GetSpawnHoleInstantiator().StopAllCoroutines();
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), GetAnchor() + Vector2.down * HALALIT_DISTANCE_BELOW);
        var enemyBank = TestUtils.GetEnemyBank();
        for (int takenGroups = 0; takenGroups < pairGroupIndex; takenGroups++)
        {
            enemyBank.GetNextSpawnHoleEnemiesList();
        }
        var spawningHolePrefab = Resources.Load<GameObject>(TestUtils.SPAWNING_HOLE_PREFAB_PATH);
        AssertWrapper.IsNotNull(spawningHolePrefab, _currentSeed, $"Test setup: no spawn hole prefab at Resources/{TestUtils.SPAWNING_HOLE_PREFAB_PATH}");

        // WHEN
        Object.Instantiate(spawningHolePrefab, GetAnchor() + Vector2.up * SPAWN_HOLE_DISTANCE_ABOVE, Quaternion.identity);
        var bodiesTimeout = Time.time + SPAWN_HOLE_PHASE_TIMEOUT;
        while (TestUtils.GetEnemies().Length == 0 && Time.time < bodiesTimeout)
        {
            yield return new WaitForFixedUpdate();
        }
        var bodies = TestUtils.GetEnemies();
        var bodyTwins = bodies.Select(body => body.GetComponent<TetherTwin>()).Where(bodyTwin => bodyTwin != null).ToList();
        var switchOnTimeout = Time.time + SPAWN_HOLE_PHASE_TIMEOUT;
        while (bodyTwins.Any(bodyTwin => !bodyTwin.enabled) && Time.time < switchOnTimeout)
        {
            yield return new WaitForFixedUpdate();
        }

        // THEN
        AssertWrapper.AreEqual(pairGroupEntries + pairGroupPairs, bodies.Length, $"The spawn hole let out {bodies.Length} bodies for {pairGroupEntries} entries with {pairGroupPairs} pairs: a paired entry is two bodies", _currentSeed);
        AssertWrapper.AreEqual(2 * pairGroupPairs, bodyTwins.Count, "The spawn hole did not let out both twins of every pair", _currentSeed);
        foreach (var bodyTwin in bodyTwins)
        {
            var partner = bodyTwin.GetPartner();
            AssertWrapper.IsTrue(partner != null && partner != bodyTwin && partner.GetPartner() == bodyTwin && bodyTwins.Contains(partner), "A twin from the spawn hole has no partner from the same hole", _currentSeed);
            AssertWrapper.IsTrue(bodyTwin.GetTether() != null && bodyTwin.GetTether() == partner.GetTether(), "Two twins from the spawn hole do not share one tether", _currentSeed);
        }
        AssertWrapper.IsTrue(bodyTwins.All(bodyTwin => bodyTwin.enabled), "The spawn hole never switched its twins on: it got stuck", _currentSeed);
        for (int first = 0; first < bodies.Length; first++)
        {
            for (int second = first + 1; second < bodies.Length; second++)
            {
                AssertWrapper.Greater(Vector2.Distance(bodies[first].transform.position, bodies[second].transform.position), MIN_SPAWN_POINT_GAP, "Two bodies came out of the spawn hole at one point: each body needs a spawn point of its own", _currentSeed);
            }
        }
    }

    // The card's normal loot: the scene's DropsBank has a TETHER_TWIN row that drops something, copied from LAME_ENEMY
    [UnityTest]
    public IEnumerator TetherTwinHasALootRow()
    {
        // GIVEN
        yield return null;

        // WHEN
        var tetherTwinDrops = TestUtils.GetDropsBank().GetDefaultDropsList().Find(dropperDrops => dropperDrops.Key == Dropper.TETHER_TWIN);

        // THEN
        AssertWrapper.IsTrue(tetherTwinDrops != null, "DropsBank has no TETHER_TWIN row: copy the LAME_ENEMY row (Needs Ofir in Unity, #237)", _currentSeed);
        AssertWrapper.Greater(tetherTwinDrops.Value.Count, 0, "DropsBank's TETHER_TWIN row drops nothing: copy the LAME_ENEMY row (Needs Ofir in Unity, #237)", _currentSeed);
    }

    // Test mode, with the Halalit moved below the middle of the world, where the twins go
    private IEnumerator SetUpScene()
    {
        yield return null;
        TestUtils.SetTestMode();
        _isTimeScaleChanged = true;
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), GetAnchor() + Vector2.down * HALALIT_DISTANCE_BELOW);
        yield return new WaitForFixedUpdate();
    }

    private Vector2 GetAnchor()
    {
        return TestUtils.GetInternalWorldBoxCollider2D().bounds.center;
    }

    // Two twins on a level line through the anchor, distance apart
    private List<GameObject> LoadLevelTwins(float distance, bool frozen = true)
    {
        var anchor = GetAnchor();
        return TestUtils.LoadTetherTwins(anchor + Vector2.left * distance / 2, anchor + Vector2.right * distance / 2, _currentSeed, frozen);
    }

    private Tether GetTether(List<GameObject> twins)
    {
        return twins[0].GetComponent<TetherTwin>().GetTether();
    }

    // The line's harm per touch. Above 0, or a test that expects a hit would also pass with a line that does nothing.
    private int GetTetherHarm(Tether tether)
    {
        var tetherHarm = tether.GetComponent<AttackBehaviour>().AttackStats.Power;
        AssertWrapper.Greater(tetherHarm, 0, "Tether's AttackBehaviour has no Power (Needs Ofir in Unity, #237)", _currentSeed);
        return tetherHarm;
    }

    // A second pair: its first twin at firstPosition, its second TWINS_DISTANCE along direction, seeded apart from the first pair
    private List<GameObject> LoadOtherPair(Vector2 firstPosition, Vector2 direction)
    {
        return TestUtils.LoadTetherTwins(firstPosition, firstPosition + direction * TWINS_DISTANCE, _currentSeed + OTHER_PAIR_SEED_OFFSET);
    }

    private Vector2 GetLineMiddle(List<GameObject> twins)
    {
        return (twins[0].transform.position + twins[1].transform.position) / 2f;
    }

    // Waits, at most one whole cycle, until the tether harms
    private IEnumerator WaitUntilHarmful(Tether tether)
    {
        var timeout = Time.time + tether.GetOnDuration() + tether.GetOffDuration() + TIME_MARGIN;
        while (!tether.IsHarmful() && Time.time < timeout)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    // Health must have run its Start (a frame after loading). The enemy is gone at the end of the frame.
    private void Kill(GameObject enemy)
    {
        var health = enemy.GetComponent<Health>();
        health.TryChangeHealth(-health.GetHealth());
    }

    // A prefab that comes as a pair, as the enemy bank sees it
    private bool IsPairedPrefab(GameObject prefab)
    {
        return prefab.TryGetComponent<IPairedEnemy>(out _);
    }

    private ObjectLoader GetObjectLoader()
    {
        return GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
    }

    [TearDown]
    public void TearDown()
    {
        EnemyTarget.ClearAllGrudges();
        // EnemyTarget keeps its decoys in a static list (D22)
        if (_decoy != null)
        {
            EnemyTarget.RemoveDecoy(_decoy.transform);
            _decoy = null;
        }
        TestUtils.DestroyAllGameObjects();
        // Only after a test saved a scale (SetTestMode): ResetTimeScale restores the saved scale, and that is 0 if none was saved yet
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
