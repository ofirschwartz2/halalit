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
            for (int earlierFill = 0; earlierFill < fill; earlierFill++)
            {
                AssertWrapper.IsFalse(GetSeeds(fills[fill]).SequenceEqual(GetSeeds(fills[earlierFill])), $"Fill {fill + 1} repeats the seeds of fill {earlierFill + 1}", _currentSeed);
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
        TestUtils.UnhookMenuLoadFromGameOver();
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

    // A failed spot search looks again 1 s later (#248). Before, it waited a whole gap plus 1 s, about 6 s with the scene's gaps.
    // A trigger over the whole world makes every search fail until it is removed. Time scale 1, so editor hitches do not stretch the timing.
    // The ship blocks about half the spots and a search tries 10, so very rarely a search still fails after the trigger is gone, and the hole comes 1 s late.
    [UnityTest]
    public IEnumerator FailedSpotSearchRetriesWithinASecond()
    {
        // GIVEN
        yield return null;
        yield return null;
        TestUtils.SetTestMode(keepEnemiesContainer: true);
        TestTimeController.SetCustomTimeScale(REAL_TIME_SCALE);
        _isTimeScaleChanged = true;
        TestUtils.UnhookMenuLoadFromGameOver();
        var firstGap = TestUtils.GetSpawnHoleInstantiator().GetInstantiationGap(0);
        var startTime = Time.time;
        var spotSearchBlocker = CreateSpotSearchBlocker();
        while (Time.time < startTime + firstGap + BLOCKED_SECONDS_AFTER_FIRST_GAP)
        {
            yield return null;
        }
        AssertWrapper.IsTrue(Object.FindObjectsOfType<SpawnHole>().Length == 0, "A spawn hole opened while a trigger covered the whole world", _currentSeed);

        // WHEN
        Object.Destroy(spotSearchBlocker);
        var unblockTime = Time.time;
        while (Object.FindObjectsOfType<SpawnHole>().Length == 0 && Time.time - unblockTime < MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING)
        {
            yield return null;
        }
        var secondsToHole = Time.time - unblockTime;

        // THEN
        AssertWrapper.IsTrue(Object.FindObjectsOfType<SpawnHole>().Length > 0, $"No spawn hole opened within {MAX_SECONDS_TO_HOLE_AFTER_UNBLOCKING} s after the trigger was removed", _currentSeed);
        AssertWrapper.Less(secondsToHole, FAILED_SEARCH_RETRY_SECONDS + RETRY_MARGIN, $"The spawn hole opened {secondsToHole} s after the trigger was removed, not within about {FAILED_SEARCH_RETRY_SECONDS} s", _currentSeed);
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

    private List<string> GetSortedPrefabNames(List<EnemyEntity> enemies)
    {
        return enemies.Select(enemy => enemy.Prefab.name).OrderBy(prefabName => prefabName).ToList();
    }

    private List<int> GetSeeds(List<EnemyEntity> enemies)
    {
        return enemies.Select(enemy => enemy.Seed).ToList();
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
