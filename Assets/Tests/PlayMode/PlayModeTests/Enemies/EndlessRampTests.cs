using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Endless Ramp (#248): the enemy bank refills itself, so the spawner never runs out of enemies,
// and the gaps between spawn holes step down the scene's gap list, then hold its last row.
// Gaps are read two frames after the Playground loads, once the spawner's Start has taken the daily twist.
public class EndlessRampTests
{
    private const int FILLS_TO_TAKE = 3;
    private const int HOLES_TO_PLAN = 200;
    private const int FLOOR_HOLES = 20;
    private const float GAP_DELTA = 0.0001f;
    private const float MAX_SECONDS_TO_NEW_ENEMIES = 20f;
    private const float FAILED_SEARCH_RETRY_SECONDS = 1f; // as in SpawnHoleInstantiator
    private const float RETRY_MARGIN = 0.5f;
    private const float BLOCKED_SECONDS_AFTER_FIRST_GAP = 1.5f;
    private const float MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING = 3f;
    private const int MAX_FRAMES_TO_SPAWNER_START = 10;
    private const float FIRST_HOLE_EARLY_MARGIN = 0.1f;
    private const float FIRST_HOLE_LATE_MARGIN = 0.6f; // the 0.1 s check and a few frames; a failed spot search would add 1 s
    private const float MAX_SECONDS_TO_WAIT_AFTER_FIRST_GAP = 2.5f; // long enough to see a hole at a normal day's 5 s, so that failure shows its time
    private const int HOLES_TO_PLACE = 2; // two placements, each checked for one row down
    private const float PLACEMENT_MARGIN_SECONDS = 3f; // room for a few failed spot searches, 1 s each; each hole is removed before its enemies come out, so searches rarely fail
    private const float REAL_TIME_SCALE = 1f;
    private const string SPOT_SEARCH_BLOCKER_NAME = "SpotSearchBlocker";
    private int _currentSeed;
    private bool _isTimeScaleChanged;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        TestUtils.SetTestTwist(DailyTwistName.NONE);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator EveryRefillHasTheSameEnemiesWithNewSeeds()
    {
        // GIVEN
        yield return null;
        var enemyBank = TestUtils.GetEnemyBank();
        var enemiesPerFill = enemyBank.GetEnemiesPerFill();

        // WHEN
        var enemies = TestUtils.TakeEnemyGroups(enemyBank, FILLS_TO_TAKE, _currentSeed).SelectMany(enemyGroup => enemyGroup).ToList();

        // THEN
        var fills = new List<List<EnemyEntity>>();
        for (int fill = 0; fill < FILLS_TO_TAKE; fill++)
        {
            fills.Add(enemies.Skip(fill * enemiesPerFill).Take(enemiesPerFill).ToList());
        }

        for (int fill = 1; fill < FILLS_TO_TAKE; fill++)
        {
            AssertWrapper.IsTrue(GetSortedPrefabNames(fills[fill]).SequenceEqual(GetSortedPrefabNames(fills[0])), $"Fill {fill + 1} has other enemies than fill 1, the scene's bank", _currentSeed);
            // Shares no seed at all, partner seeds included (#237), so a refill that replays a saved list in a new order also fails. Seeds are drawn from 0 to 2^31, so a chance match is under 1 in a million.
            for (int earlierFill = 0; earlierFill < fill; earlierFill++)
            {
                var sharedSeeds = GetSeeds(fills[fill]).Intersect(GetSeeds(fills[earlierFill])).Count();
                AssertWrapper.IsTrue(sharedSeeds == 0, $"Fill {fill + 1} reuses {sharedSeeds} seeds of fill {earlierFill + 1}: a refill must draw new seeds, not replay a list", _currentSeed);
            }
        }
    }

    // Before #248 the spawner removed itself, with its bank, at its first spawn time once the bank was empty
    [UnityTest]
    public IEnumerator SpawnerKeepsOpeningHolesAfterTheBankRunsOut()
    {
        // GIVEN
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        _isTimeScaleChanged = true;
        TestUtils.UnhookResultsCardFromGameOver();
        var enemyBank = TestUtils.GetEnemyBank();
        TestUtils.TakeEnemyGroups(enemyBank, 1, _currentSeed);
        AssertWrapper.IsFalse(enemyBank.NoMoreEnemies(), "The enemy bank was empty after its whole first fill was taken: it did not refill", _currentSeed);
        var enemiesAtStart = TestUtils.GetEnemies().Length;
        var startTime = Time.time;
        var didSpawnHoleOpen = false;
        var didEnemiesArrive = false;

        // WHEN
        while (!(didSpawnHoleOpen && didEnemiesArrive) && Time.time - startTime < MAX_SECONDS_TO_NEW_ENEMIES)
        {
            yield return null;
            didSpawnHoleOpen = didSpawnHoleOpen || Object.FindObjectsOfType<SpawnHole>().Length > 0;
            didEnemiesArrive = didEnemiesArrive || TestUtils.GetEnemies().Length > enemiesAtStart;
        }

        // THEN
        AssertWrapper.IsTrue(enemyBank != null, "The spawner removed itself, and its enemy bank with it", _currentSeed);
        AssertWrapper.IsTrue(didSpawnHoleOpen, $"No spawn hole opened within {MAX_SECONDS_TO_NEW_ENEMIES} s after the first fill was taken", _currentSeed);
        AssertWrapper.IsTrue(didEnemiesArrive, $"No new enemies came within {MAX_SECONDS_TO_NEW_ENEMIES} s after the first fill was taken", _currentSeed);
    }

    // Checks the shape only, so the numbers can be retuned in the scene without a code change
    [UnityTest]
    public IEnumerator PlannedGapsStepDownToTheFloorAndHold()
    {
        // GIVEN
        yield return null;
        yield return null;

        // WHEN
        var gaps = GetPlannedGaps();
        var plannedHoleTimes = GetPlannedHoleTimes(gaps);

        // THEN
        var lastGap = gaps[HOLES_TO_PLAN - 1];
        AssertWrapper.Greater(gaps[0], lastGap, $"The first gap, {gaps[0]} s, is not longer than the last, {lastGap} s: type the #248 gap list into Instantiation Rate (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.Greater(lastGap, 0, "The last gap between spawn holes is not above 0 s", _currentSeed);
        for (int hole = 1; hole < HOLES_TO_PLAN; hole++)
        {
            AssertWrapper.GreaterOrEqual(gaps[hole - 1], gaps[hole], $"The gap rises from {gaps[hole - 1]} s to {gaps[hole]} s before hole {hole}, planned at {plannedHoleTimes[hole]:0.#} s", _currentSeed, GAP_DELTA);
        }
        for (int hole = HOLES_TO_PLAN - FLOOR_HOLES; hole < HOLES_TO_PLAN; hole++)
        {
            AssertWrapper.AreEqual(lastGap, gaps[hole], $"The gap before hole {hole}, planned at {plannedHoleTimes[hole]:0.#} s, is {gaps[hole]} s, not the floor of {lastGap} s", _currentSeed, GAP_DELTA);
        }
    }

    // Catches a Swarm multiplier that is lost or applied twice
    [UnityTest]
    public IEnumerator SwarmCutsEveryPlannedGapTo60Percent()
    {
        // GIVEN
        yield return null;
        yield return null;
        var plainGaps = GetPlannedGaps();

        // WHEN
        TestUtils.SetRandomSeed(_currentSeed);
        TestUtils.SetTestTwist(DailyTwistName.SWARM);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;
        yield return null;
        var swarmGaps = GetPlannedGaps();

        // THEN
        var swarmMultiplier = DailyTwists.Get(DailyTwistName.SWARM).SpawnIntervalMultiplier;
        for (int hole = 0; hole < HOLES_TO_PLAN; hole++)
        {
            AssertWrapper.AreEqual(plainGaps[hole] * swarmMultiplier, swarmGaps[hole], $"On Swarm the gap before hole {hole} is {swarmGaps[hole]} s, not {swarmMultiplier} of {plainGaps[hole]} s", _currentSeed, GAP_DELTA);
        }
    }

    // The spawner's real timing follows its planned gap, Swarm's cut included: no hole before the first gap ends, and one soon after.
    // Replaces #250's SwarmFirstSpawnComesAfter3Seconds, which emptied the bank in a loop that never ends now that the bank refills.
    // The ship is parked outside the world and asteroids are off, so the first spot search finds a place. Time scale 1, so editor hitches do not stretch the timing.
    [UnityTest]
    public IEnumerator SwarmFirstSpawnHoleOpensWhenItsCutGapEnds()
    {
        // GIVEN
        yield return null;
        yield return null;
        var swarmMultiplier = DailyTwists.Get(DailyTwistName.SWARM).SpawnIntervalMultiplier;
        TestUtils.SetRandomSeed(_currentSeed);
        TestUtils.SetTestTwist(DailyTwistName.SWARM);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        // Start runs in the frame the scene loads, before the test resumes, so the first frame that sees Swarm's multiplier has the time the first gap counts from
        var framesAfterLoad = 0;
        do
        {
            yield return null;
            framesAfterLoad++;
        } while (!DidSpawnerStartWith(swarmMultiplier) && framesAfterLoad < MAX_FRAMES_TO_SPAWNER_START);
        var spawnerStartTime = Time.time;
        AssertWrapper.IsTrue(DidSpawnerStartWith(swarmMultiplier), $"The spawner had not started with Swarm's multiplier {framesAfterLoad} frames after the Playground loaded", _currentSeed);
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        TestTimeController.SetCustomTimeScale(REAL_TIME_SCALE);
        _isTimeScaleChanged = true;
        TestUtils.UnhookResultsCardFromGameOver();
        ParkHalalitOutsideTheWorld();
        var firstGap = TestUtils.GetSpawnHoleInstantiator().GetInstantiationGap(0);

        // WHEN
        while (!IsAnySpawnHoleOpen() && Time.time - spawnerStartTime < firstGap + MAX_SECONDS_TO_WAIT_AFTER_FIRST_GAP)
        {
            yield return null;
        }
        var secondsToFirstHole = Time.time - spawnerStartTime;

        // THEN
        AssertWrapper.IsTrue(IsAnySpawnHoleOpen(), $"No spawn hole opened within {firstGap + MAX_SECONDS_TO_WAIT_AFTER_FIRST_GAP} s on Swarm, whose first gap is {firstGap} s", _currentSeed);
        AssertWrapper.Greater(secondsToFirstHole, firstGap - FIRST_HOLE_EARLY_MARGIN, $"On Swarm the first spawn hole opened after {secondsToFirstHole:0.00} s, before its gap of {firstGap} s ended", _currentSeed);
        AssertWrapper.Less(secondsToFirstHole, firstGap + FIRST_HOLE_LATE_MARGIN, $"On Swarm the first spawn hole opened after {secondsToFirstHole:0.00} s, not within {FIRST_HOLE_LATE_MARGIN} s after its gap of {firstGap} s", _currentSeed);
    }

    // A failed spot search looks again 1 s later (#248). Before, it waited a whole gap plus 1 s, about 6 s with the scene's gaps.
    // A trigger over the whole world makes every search fail until it is removed. Time scale 1, so editor hitches do not stretch the timing.
    // The ship is parked outside the world first. In the middle, with the edge force fields, it blocks 70% of the spots,
    // so about 1 search in 35 would still fail after the trigger is gone, and the hole would come 1 s late.
    [UnityTest]
    public IEnumerator FailedSpotSearchRetriesWithinASecond()
    {
        // GIVEN
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        TestTimeController.SetCustomTimeScale(REAL_TIME_SCALE);
        _isTimeScaleChanged = true;
        TestUtils.UnhookResultsCardFromGameOver();
        ParkHalalitOutsideTheWorld();
        var firstGap = TestUtils.GetSpawnHoleInstantiator().GetInstantiationGap(0);
        var startTime = Time.time;
        var spotSearchBlocker = CreateSpotSearchBlocker();
        var didHoleOpenWhileBlocked = false;
        while (Time.time < startTime + firstGap + BLOCKED_SECONDS_AFTER_FIRST_GAP)
        {
            yield return null;
            didHoleOpenWhileBlocked = didHoleOpenWhileBlocked || IsAnySpawnHoleOpen();
        }
        AssertWrapper.IsFalse(didHoleOpenWhileBlocked, "A spawn hole opened while a trigger covered the whole world", _currentSeed);

        // WHEN
        Object.Destroy(spotSearchBlocker);
        var unblockTime = Time.time;
        while (!IsAnySpawnHoleOpen() && Time.time - unblockTime < MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING)
        {
            yield return null;
        }
        var secondsToHole = Time.time - unblockTime;

        // THEN
        AssertWrapper.IsTrue(IsAnySpawnHoleOpen(), $"No spawn hole opened within {MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING} s after the trigger was removed", _currentSeed);
        AssertWrapper.Less(secondsToHole, FAILED_SEARCH_RETRY_SECONDS + RETRY_MARGIN, $"The spawn hole opened {secondsToHole} s after the trigger was removed, not within about {FAILED_SEARCH_RETRY_SECONDS} s", _currentSeed);
    }

    // A failed spot search keeps its row of the gap list (#248): only a placed hole moves the list on, so a crowded arena does not skip steps of the ramp.
    // Before #248 every try used up a row. The hole that opens once the trigger is gone shows the spawner was searching all along.
    // The ship is parked outside the world, so that first search after the trigger finds a place.
    [UnityTest]
    public IEnumerator FailedSpotSearchKeepsItsRowOfTheGapList()
    {
        // GIVEN
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        _isTimeScaleChanged = true;
        TestUtils.UnhookResultsCardFromGameOver();
        ParkHalalitOutsideTheWorld();
        var spawnHoleInstantiator = TestUtils.GetSpawnHoleInstantiator();
        AssertGapListIsTyped(spawnHoleInstantiator);
        var rowBeforeSearches = spawnHoleInstantiator.GetInstantiationRatesIndex();
        var firstGap = spawnHoleInstantiator.GetInstantiationGap(0);
        var startTime = Time.time;
        var spotSearchBlocker = CreateSpotSearchBlocker();

        // WHEN
        var didHoleOpenWhileBlocked = false;
        while (Time.time < startTime + firstGap + BLOCKED_SECONDS_AFTER_FIRST_GAP)
        {
            yield return null;
            didHoleOpenWhileBlocked = didHoleOpenWhileBlocked || IsAnySpawnHoleOpen();
        }
        var rowAfterFailedSearches = spawnHoleInstantiator.GetInstantiationRatesIndex();
        Object.Destroy(spotSearchBlocker);
        var unblockTime = Time.time;
        while (!IsAnySpawnHoleOpen() && Time.time - unblockTime < MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING)
        {
            yield return null;
        }
        var rowAfterPlacedHole = spawnHoleInstantiator.GetInstantiationRatesIndex();

        // THEN
        AssertWrapper.IsFalse(didHoleOpenWhileBlocked, "A spawn hole opened while a trigger covered the whole world", _currentSeed);
        AssertWrapper.IsTrue(IsAnySpawnHoleOpen(), $"No spawn hole opened within {MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING} s after the trigger was removed, so the spawner may not have been searching", _currentSeed);
        AssertWrapper.AreEqual(rowBeforeSearches, rowAfterFailedSearches, $"Failed spot searches moved the gap list from row {rowBeforeSearches} to row {rowAfterFailedSearches}: a failed search must keep its row", _currentSeed);
        AssertWrapper.AreEqual(rowBeforeSearches + 1, rowAfterPlacedHole, $"The hole placed after the failed searches moved the gap list from row {rowBeforeSearches} to row {rowAfterPlacedHole}, not one row down", _currentSeed);
    }

    // Each placed hole moves the gap list one row down, so the gap after it is the next row of the list (#248).
    // The ship is parked outside the world, and each hole is removed as soon as it is seen, before its enemies come out 1 s later.
    // So nothing blocks the next spot search.
    [UnityTest]
    public IEnumerator EachPlacedHoleMovesOneRowDown()
    {
        // GIVEN
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        _isTimeScaleChanged = true;
        TestUtils.UnhookResultsCardFromGameOver();
        ParkHalalitOutsideTheWorld();
        var spawnHoleInstantiator = TestUtils.GetSpawnHoleInstantiator();
        AssertGapListIsTyped(spawnHoleInstantiator);
        var rowBeforeFirstHole = spawnHoleInstantiator.GetInstantiationRatesIndex();
        var maxSecondsToPlaceHoles = GetPlannedHoleTimes(GetPlannedGaps())[HOLES_TO_PLACE - 1] + PLACEMENT_MARGIN_SECONDS;
        var startTime = Time.time;
        var seenSpawnHoles = new HashSet<int>();
        var rowsAfterPlacedHoles = new List<int>();

        // WHEN
        while (rowsAfterPlacedHoles.Count < HOLES_TO_PLACE && Time.time - startTime < maxSecondsToPlaceHoles)
        {
            yield return null;
            foreach (var spawnHole in Object.FindObjectsOfType<SpawnHole>())
            {
                if (seenSpawnHoles.Add(spawnHole.GetInstanceID()))
                {
                    rowsAfterPlacedHoles.Add(spawnHoleInstantiator.GetInstantiationRatesIndex());
                    Object.Destroy(spawnHole.gameObject);
                }
            }
        }

        // THEN
        AssertWrapper.AreEqual(HOLES_TO_PLACE, rowsAfterPlacedHoles.Count, $"{rowsAfterPlacedHoles.Count} spawn holes opened within {maxSecondsToPlaceHoles} s, not {HOLES_TO_PLACE}", _currentSeed);
        for (int hole = 0; hole < HOLES_TO_PLACE; hole++)
        {
            var expectedRow = rowBeforeFirstHole + hole + 1;
            AssertWrapper.AreEqual(expectedRow, rowsAfterPlacedHoles[hole], $"Hole {hole} left the gap list at row {rowsAfterPlacedHoles[hole]}, not row {expectedRow}: each placed hole moves it one row down", _currentSeed);
        }
    }

    // The gap before each of the first HOLES_TO_PLAN holes, as the spawner plans it
    private List<float> GetPlannedGaps()
    {
        var spawnHoleInstantiator = TestUtils.GetSpawnHoleInstantiator();
        var gaps = new List<float>();
        for (int hole = 0; hole < HOLES_TO_PLAN; hole++)
        {
            gaps.Add(spawnHoleInstantiator.GetInstantiationGap(hole));
        }

        return gaps;
    }

    // When each hole is planned, counted from the spawner's start, if every spot search finds a free place
    private List<float> GetPlannedHoleTimes(List<float> gaps)
    {
        var plannedHoleTimes = new List<float>();
        var plannedHoleTime = 0f;
        foreach (var gap in gaps)
        {
            plannedHoleTime += gap;
            plannedHoleTimes.Add(plannedHoleTime);
        }

        return plannedHoleTimes;
    }

    // The row of the gap list moves only while rows are left before the floor's row, and the scene's single row of 5 s keeps it at 0.
    // So until the #248 list is typed in, the row tests would pass empty: they fail here instead.
    private void AssertGapListIsTyped(SpawnHoleInstantiator spawnHoleInstantiator)
    {
        var lastRowTheTestsReach = HOLES_TO_PLACE + 1;
        var floorGap = spawnHoleInstantiator.GetInstantiationGap(HOLES_TO_PLAN - 1);
        AssertWrapper.IsTrue(spawnHoleInstantiator.GetInstantiationGap(lastRowTheTestsReach) > floorGap, $"The gap list is at its floor of {floorGap} s by row {lastRowTheTestsReach}, so its row cannot move: type the #248 gap list into Instantiation Rate (Needs Ofir in Unity, #248)", _currentSeed);
    }

    private List<string> GetSortedPrefabNames(List<EnemyEntity> enemies)
    {
        return enemies.Select(enemy => enemy.Prefab.name).OrderBy(prefabName => prefabName).ToList();
    }

    // Tether Twins (#237): a paired entry's partner seed is a seed too, and a refill must draw it anew
    private List<int> GetSeeds(List<EnemyEntity> enemies)
    {
        return enemies.Select(enemy => enemy.Seed)
            .Concat(enemies.Where(enemy => enemy.HasPartner).Select(enemy => enemy.PartnerSeed))
            .ToList();
    }

    private bool IsAnySpawnHoleOpen()
    {
        return Object.FindObjectsOfType<SpawnHole>().Length > 0;
    }

    // True once the spawner of the scene loaded last has run Start: before Start its multiplier is 0, and the scene loaded before had another twist
    private bool DidSpawnerStartWith(float spawnIntervalMultiplier)
    {
        var enemiesSpawner = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription());
        return enemiesSpawner != null && Mathf.Abs(enemiesSpawner.GetComponent<SpawnHoleInstantiator>().GetSpawnIntervalMultiplier() - spawnIntervalMultiplier) < GAP_DELTA;
    }

    // ObjectLoader's parking place, outside the world, where the ship stays alive (ObjectLoaderTests). With the ship there and nothing else in the arena,
    // only the edge force fields block spots, and a 10-try search fails about once in 45,000.
    private void ParkHalalitOutsideTheWorld()
    {
        GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>().MoveHalalitToExternalSafeIsland();
    }

    // An untagged trigger over the whole internal world. Every spot the spawner tries touches it, and nothing in the game reacts to an untagged trigger.
    private GameObject CreateSpotSearchBlocker()
    {
        var internalWorldBounds = TestUtils.GetInternalWorldBoxCollider2D().bounds;
        var spotSearchBlocker = new GameObject(SPOT_SEARCH_BLOCKER_NAME);
        spotSearchBlocker.transform.position = internalWorldBounds.center;
        var blockerCollider = spotSearchBlocker.AddComponent<BoxCollider2D>();
        blockerCollider.isTrigger = true;
        blockerCollider.size = internalWorldBounds.size;
        return spotSearchBlocker;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.ResetTestTwist();
        TestUtils.DestroyAllGameObjects();
        // Only after a test saved a scale (SetTestMode): ResetTimeScale restores the saved scale, and that is 0 if none was saved yet
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
