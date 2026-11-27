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
public class DailyTwistTests
{
    private const int SCENE_START_HEALTH = 30;
    private const int GLASS_DAY_START_HEALTH = 12;
    private const float SWARM_SPAWN_INTERVAL_MULTIPLIER = 0.6f;
    private const float BIG_ROCKS_ASTEROID_MIN_SCALE = 3f;
    private const float KNOB_DELTA = 0.0001f;
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

    [TearDown]
    public void TearDown()
    {
        TestUtils.ResetTestTwist();
        TestUtils.DestroyAllGameObjects();
        // Only after SetTestMode: ResetTimeScale restores the scale SetTestMode saved, and that is 0 if no test saved one yet
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
