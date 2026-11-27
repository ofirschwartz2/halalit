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
    private int _currentSeed;
    private bool _isTimeScaleChanged;

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
        var tetherHarm = tether.GetComponent<AttackBehaviour>().AttackStats.Power;
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
        var tetherHarm = tether.GetComponent<AttackBehaviour>().AttackStats.Power;
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

    // Turned a quarter, a twin's long body (Lame's capsule) lies along the line, as near to the harm segment as it gets
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
        yield return null;
        foreach (var twin in twins)
        {
            twin.GetComponent<Health>().SetHealth(TWIN_HEALTH);
        }

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
        AssertWrapper.Greater(harmStartDistance, survivor.GetComponent<TetherTwin>().GetBodyRadius(), "The flail's harm starts inside the survivor's body", _currentSeed);
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
        TestUtils.DestroyAllGameObjects();
        // Only after a test saved a scale (SetTestMode): ResetTimeScale restores the saved scale, and that is 0 if none was saved yet
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
