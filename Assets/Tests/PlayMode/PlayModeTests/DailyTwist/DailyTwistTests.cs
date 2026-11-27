using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Daily Twist (#239): OriginalRandomSeed sets the run's twist when the Playground loads,
// so each test sets its twist first and then loads the scene itself.
// The two player-run tests load it as a player's daily or PLAY run, outside test mode.
public class DailyTwistTests
{
    private const int SCENE_START_HEALTH = 30;
    private const int GLASS_DAY_START_HEALTH = 12;
    private const float SWARM_SPAWN_INTERVAL_MULTIPLIER = 0.6f;
    private const float BIG_ROCKS_ASTEROID_MIN_SCALE = 3f;
    private const float KNOB_DELTA = 0.0001f;
    private const float SCENE_SPAWN_INTERVAL = 5f;
    private const float SWARM_SPAWN_INTERVAL = 3f;
    private const float SPAWN_TIME_DELTA = 0.5f;
    private const float MAX_SECONDS_TO_FIRST_SPAWN = 10f;
    private const float REAL_TIME_SCALE = 1f;
    private const float SCENE_ASTEROID_MIN_SCALE = 1f;
    private const float ASTEROID_SCALE_TOP = 5f; // the scene's _asteroidMaxScale 4, plus 1
    private const float ASTEROID_SCALE_DELTA = 0.001f;
    private const float MAX_SECONDS_TO_FIRST_ASTEROID = 10f;
    private const int DAILY_BLOCK_DAYS = 3; // one day per twist
    private static readonly System.DateTime DAILY_DATE = new System.DateTime(2026, 11, 23); // a negative daily seed: -1359240184
    private static readonly System.DateTime DAILY_BLOCK_FIRST_DATE = new System.DateTime(2026, 11, 22); // 22 to 24 Nov 2026: Big Rocks, Swarm, Glass Day
    private int _currentSeed;
    private bool _isTimeScaleChanged;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
    }

    [UnityTest]
    public IEnumerator GlassDayShipStartsWith12Health()
    {
        // GIVEN
        TestUtils.SetTestTwist(DailyTwistName.GLASS_DAY);

        // WHEN
        yield return LoadPlaygroundInTestMode();

        // THEN
        AssertWrapper.AreEqual(GLASS_DAY_START_HEALTH, GetHalalitHealth(), "Glass Day ship did not start with 12 health. Is Follows Daily Twist ticked on the Halalit's Health? (Needs Ofir in Unity)", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NoTwistShipKeepsSceneHealth()
    {
        // GIVEN
        TestUtils.SetTestTwist(DailyTwistName.NONE);

        // WHEN
        yield return LoadPlaygroundInTestMode();

        // THEN
        AssertWrapper.AreEqual(SCENE_START_HEALTH, GetHalalitHealth(), "Ship without a twist did not keep the scene's 30 health", _currentSeed);
    }

    [UnityTest]
    public IEnumerator StaleTwistIsReplacedOnLoad()
    {
        // GIVEN
        DailyTwists.SetCurrent(DailyTwists.Get(DailyTwistName.GLASS_DAY));
        TestUtils.SetTestTwist(DailyTwistName.NONE);

        // WHEN
        yield return LoadPlaygroundInTestMode();

        // THEN
        AssertWrapper.IsTrue(DailyTwists.Current.IsNone(), $"The twist of an earlier run, {DailyTwists.Current.Name}, stayed after the Playground loaded", _currentSeed);
        AssertWrapper.AreEqual(SCENE_START_HEALTH, GetHalalitHealth(), "The twist of an earlier run changed the ship's health", _currentSeed);
    }

    [UnityTest]
    public IEnumerator SwarmShortensSpawnInterval()
    {
        // GIVEN
        TestUtils.SetTestTwist(DailyTwistName.SWARM);

        // WHEN
        yield return LoadPlayground();

        // THEN
        var spawnHoleInstantiator = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription()).GetComponent<SpawnHoleInstantiator>();
        AssertWrapper.AreEqual(SWARM_SPAWN_INTERVAL_MULTIPLIER, spawnHoleInstantiator.GetSpawnIntervalMultiplier(), "Swarm did not shorten the spawn interval to 0.6 of the scene's", _currentSeed, KNOB_DELTA);
    }

    [UnityTest]
    public IEnumerator BigRocksRaisesMinimumScale()
    {
        // GIVEN
        TestUtils.SetTestTwist(DailyTwistName.BIG_ROCKS);

        // WHEN
        yield return LoadPlayground();

        // THEN
        var asteroidExternalInstantiator = GameObject.FindGameObjectWithTag(Tag.ASTEROID_INSTANTIATOR.GetDescription()).GetComponent<AsteroidExternalInstantiator>();
        AssertWrapper.AreEqual(BIG_ROCKS_ASTEROID_MIN_SCALE, asteroidExternalInstantiator.GetAsteroidMinScale(), "Big Rocks did not raise the smallest asteroid scale to 3", _currentSeed, KNOB_DELTA);
    }

    // A player's daily, not a test run: the twist comes from the daily's date in PlayerStats.
    // One daily for each day of a block (#273). Each of its days has a different twist, so a pick that ignored the daily's date could not pass
    [UnityTest]
    public IEnumerator DailyRunPlaysItsDatesTwist()
    {
        for (var i = 0; i < DAILY_BLOCK_DAYS; i++)
        {
            // GIVEN
            var dailyDate = DAILY_BLOCK_FIRST_DATE.AddDays(i);
            DailyTwists.SetCurrent(DailyTwists.Get(DailyTwistName.NONE));
            TestUtils.SetPlayerRun(true, dailyDate);

            // WHEN
            yield return LoadPlayground();

            // THEN
            var datesTwist = DailyTwists.PickForDate(dailyDate);
            AssertWrapper.IsTrue(DailyTwists.Current.Name == datesTwist.Name, $"The daily of {dailyDate:yyyy-MM-dd} played {DailyTwists.Current.Name}, not its date's {datesTwist.Name}", _currentSeed);
        }
    }

    // PLAY after a daily: the daily's twist is still the run's twist and the daily's date and seed are still set, but this run is not a daily
    [UnityTest]
    public IEnumerator PlayAfterADailyHasNoTwist()
    {
        // GIVEN
        DailyTwists.SetCurrent(DailyTwists.PickForDate(DAILY_DATE));
        TestUtils.SetPlayerRun(false, DAILY_DATE);

        // WHEN
        yield return LoadPlayground();

        // THEN
        AssertWrapper.IsTrue(DailyTwists.Current.IsNone(), $"PLAY after a daily kept the daily's twist, {DailyTwists.Current.Name}", _currentSeed);
    }

    // With the enemy bank empty, the spawner removes itself at its first spawn time instead of looking for a free place,
    // so the time is exact: 3 s on Swarm, 5 s on a normal day. It looks once a second.
    [UnityTest]
    public IEnumerator SwarmFirstSpawnComesAfter3Seconds()
    {
        // GIVEN
        UseRealTimeScale();
        TestUtils.SetTestTwist(DailyTwistName.SWARM);
        yield return LoadPlayground();
        var startTime = Time.time;
        var spawnHolesAtStart = Object.FindObjectsOfType<SpawnHole>().Length;
        var enemySpawner = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription());
        var enemyBank = enemySpawner.GetComponent<EnemyBank>();
        while (!enemyBank.NoMoreEnemies())
        {
            enemyBank.GetNextSpawnHoleEnemiesList();
        }

        // WHEN
        while (enemySpawner != null && Time.time - startTime < MAX_SECONDS_TO_FIRST_SPAWN)
        {
            yield return null;
        }
        var secondsToFirstSpawn = Time.time - startTime;

        // THEN
        AssertWrapper.IsTrue(spawnHolesAtStart == 0, "A spawn hole opened as the Playground loaded, before Swarm's first spawn time", _currentSeed);
        AssertWrapper.Greater(secondsToFirstSpawn, SWARM_SPAWN_INTERVAL - SPAWN_TIME_DELTA, $"Swarm's first spawn came after {secondsToFirstSpawn} s, before its 3 s", _currentSeed);
        AssertWrapper.Less(secondsToFirstSpawn, SCENE_SPAWN_INTERVAL - SPAWN_TIME_DELTA, $"Swarm's first spawn came after {secondsToFirstSpawn} s, no sooner than a normal day's 5 s", _currentSeed);
    }

    // The first asteroid's size is one seeded draw, from 1 to 5 on a normal day. With the same seed, Big Rocks puts that same draw on 3 to 5
    [UnityTest]
    public IEnumerator BigRocksFirstAsteroidTakesTheSameDrawFrom3To5()
    {
        // GIVEN
        SpeedUpTime();
        TestUtils.SetTestTwist(DailyTwistName.NONE);
        yield return LoadPlayground();
        yield return WaitForFirstAsteroid();
        var noTwistScale = GetFirstAsteroidScale();

        // WHEN
        TestUtils.SetRandomSeed(_currentSeed);
        TestUtils.SetTestTwist(DailyTwistName.BIG_ROCKS);
        yield return LoadPlayground();
        yield return WaitForFirstAsteroid();
        var bigRocksScale = GetFirstAsteroidScale();

        // THEN
        var drawnFraction = (noTwistScale - SCENE_ASTEROID_MIN_SCALE) / (ASTEROID_SCALE_TOP - SCENE_ASTEROID_MIN_SCALE);
        var expectedBigRocksScale = BIG_ROCKS_ASTEROID_MIN_SCALE + drawnFraction * (ASTEROID_SCALE_TOP - BIG_ROCKS_ASTEROID_MIN_SCALE);
        AssertWrapper.AreEqual(expectedBigRocksScale, bigRocksScale, $"With the same seed the first asteroid was size {noTwistScale} on a normal day, so on Big Rocks it should be {expectedBigRocksScale}", _currentSeed, ASTEROID_SCALE_DELTA);
    }

    // Loads the Playground with the twist set above and waits until Start has run on every scene object.
    // No SetTestMode: it switches off EnemiesContainer and AsteroidsContainer, whose spawners read the twist in Start.
    private IEnumerator LoadPlayground()
    {
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;
        yield return null;
    }

    // Loads the Playground with the twist set above, without enemies or asteroids, and waits until the Halalit's Start has run.
    private IEnumerator LoadPlaygroundInTestMode()
    {
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;
        TestUtils.SetTestMode();
        _isTimeScaleChanged = true;
        yield return null;
    }

    private int GetHalalitHealth()
    {
        return TestUtils.GetHalalit().GetComponent<Health>().GetHealth();
    }

    // Asteroids fly in one at a time, every 3 s from the start
    private IEnumerator WaitForFirstAsteroid()
    {
        var startTime = Time.time;
        while (TestUtils.GetAsteroids().Length == 0 && Time.time - startTime < MAX_SECONDS_TO_FIRST_ASTEROID)
        {
            yield return null;
        }
    }

    private float GetFirstAsteroidScale()
    {
        var asteroids = TestUtils.GetAsteroids();
        AssertWrapper.IsTrue(asteroids.Length == 1, $"Expected the first asteroid alone within {MAX_SECONDS_TO_FIRST_ASTEROID} s, found {asteroids.Length}", _currentSeed);
        return asteroids[0].transform.localScale.x;
    }

    // Time scale 5, as SetTestMode sets it. TearDown puts back the scale this saved
    private void SpeedUpTime()
    {
        TestTimeController.SetTestTimeScale();
        _isTimeScaleChanged = true;
    }

    // Time scale 1, whatever an earlier test left (D22), so a timed check is not stretched by editor hitches. TearDown puts back the scale this saved
    private void UseRealTimeScale()
    {
        TestTimeController.SetTestTimeScale();
        TestTimeController.SetCustomTimeScale(REAL_TIME_SCALE);
        _isTimeScaleChanged = true;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.ResetTestTwist();
        TestUtils.ResetPlayerRun();
        TestUtils.DestroyAllGameObjects();
        // Only after a test saved a scale (SetTestMode, SpeedUpTime, UseRealTimeScale): ResetTimeScale restores the saved scale, and that is 0 if none was saved yet
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
