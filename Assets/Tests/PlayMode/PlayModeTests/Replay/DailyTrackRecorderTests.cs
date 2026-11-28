using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class DailyTrackRecorderTests
{
    private const float MOVE_SECONDS = 2f;
    // Sample 0 at the start and the death sample are both extra to the regular samples
    private const int ACCEPTED_SAMPLE_COUNT_DELTA = 2;
    // The score is sampled at the first sample after the pickup. Two samples allow for the recorder's clock starting one frame after the load
    private const float ACCEPTED_SCORE_TIMING_DELTA = 2 * Constants.DAILY_TRACK_SAMPLE_SECONDS;
    private const float ONE_STEP = 1f / 255f;

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator DailyRunHandsTrackToPlayerStats()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        AssertRecorderIsOnTheHalalit();
        var halalitMovement = TestUtils.GetHalalitMovement();

        // WHEN
        // The ship flies right for a while and collects a valuable
        var moveEndTime = Time.time + MOVE_SECONDS;
        while (Time.time < moveEndTime)
        {
            halalitMovement.TryMove(1f, 0f, Time.deltaTime);
            yield return null;
        }

        var pickupStartSeconds = Time.timeSinceLevelLoad;
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var pickupEndSeconds = Time.timeSinceLevelLoad;
        var shownScore = TestUtils.GetScore();
        AssertWrapper.Greater(shownScore, 0, "The valuable was not collected, so the score timeline cannot be checked", _currentSeed);

        // GIVEN
        // A death must not open the results card in this test, because the card saves the scores (#249).
        // The helper takes the card off the game over event. The recorder keeps its own handler. It takes effect one frame later.
        TestUtils.UnhookResultsCardFromGameOver();
        yield return null;

        // WHEN
        // A daily run ends. Reset, kill, read and reset are one step with no yield.
        // The finally block resets PlayerStats even when the kill throws, so no test track is left behind
        string trackJson;
        float timeAtDeath;
        Vector2 deathPosition;
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();
        try
        {
            deathPosition = TestUtils.GetHalalit().transform.position;
            TestUtils.KillHalalit();
            trackJson = PlayerStats._dailyTrack;
            timeAtDeath = Time.timeSinceLevelLoad;
        }
        finally
        {
            PlayerStats.SetDefaultPlayerStatsValues();
        }
        yield return null;

        // THEN
        var track = DailyTrackCodec.FromJson(trackJson);
        AssertWrapper.IsTrue(track != null, "A daily run handed over no track", _currentSeed);

        var positions = DailyTrackCodec.GetPositions(track);
        var expectedSamples = track.SurvivalSeconds / Constants.DAILY_TRACK_SAMPLE_SECONDS + 1;
        AssertWrapper.AreEqual(expectedSamples, positions.Length, $"The track has {positions.Length} samples for {track.SurvivalSeconds} seconds", _currentSeed, ACCEPTED_SAMPLE_COUNT_DELTA);
        AssertWrapper.AreEqual(timeAtDeath, track.SurvivalSeconds, "The survival time is not the time the ship lived", _currentSeed, Constants.DAILY_TRACK_SAMPLE_SECONDS);
        AssertWrapper.Greater(positions[positions.Length - 1].x, positions[0].x, "The track does not show the ship flying right", _currentSeed);

        AssertWrapper.IsTrue(track.Scores.Length >= 1, "The track has no score change after a valuable was collected", _currentSeed);
        AssertWrapper.IsTrue(track.Scores.Length == track.ScoreSampleIndexes.Length, "The track's scores and score sample indexes are not in pairs", _currentSeed);
        AssertWrapper.AreEqual(shownScore, track.Scores[track.Scores.Length - 1], "The track's last score is not the score shown at death", _currentSeed);

        // The sample count above follows the survival time, so a short survival would let it pass with almost no track
        AssertWrapper.GreaterOrEqual(track.SurvivalSeconds, MOVE_SECONDS, $"The survival time is {track.SurvivalSeconds} seconds, less than the {MOVE_SECONDS} seconds the ship flew", _currentSeed);

        // One valuable, so one score change, at the first sample after the pickup
        AssertWrapper.IsTrue(track.Scores.Length == 1, $"One valuable was collected, but the track has {track.Scores.Length} score changes", _currentSeed);
        var scoreChangeSeconds = track.ScoreSampleIndexes[0] * track.SampleSeconds;
        AssertWrapper.GreaterOrEqual(scoreChangeSeconds, pickupStartSeconds - ACCEPTED_SCORE_TIMING_DELTA, $"The score changed at {scoreChangeSeconds} seconds in the track, before the pickup at {pickupStartSeconds} seconds", _currentSeed);
        AssertWrapper.Less(scoreChangeSeconds, pickupEndSeconds + ACCEPTED_SCORE_TIMING_DELTA, $"The score changed at {scoreChangeSeconds} seconds in the track, long after the pickup ended at {pickupEndSeconds} seconds", _currentSeed);

        // The last sample is where the ship died
        var worldBounds = TestUtils.GetInternalWorldBoxCollider2D().bounds;
        var expectedDeathPosition = new Vector2(
            Mathf.InverseLerp(worldBounds.min.x, worldBounds.max.x, deathPosition.x),
            Mathf.InverseLerp(worldBounds.min.y, worldBounds.max.y, deathPosition.y));
        AssertWrapper.AreEqual(expectedDeathPosition, positions[positions.Length - 1], "The track's last sample is not where the ship died", _currentSeed, ONE_STEP);
    }

    [UnityTest]
    public IEnumerator NormalRunHandsOverNoTrack()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        AssertRecorderIsOnTheHalalit();
        yield return new WaitForSeconds(1);

        // A score above 0 makes Score set the high score on game over, which shows that the game over reached its listeners
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var shownScore = TestUtils.GetScore();
        AssertWrapper.Greater(shownScore, 0, "The valuable was not collected, so the game over cannot be shown to have run", _currentSeed);

        TestUtils.UnhookResultsCardFromGameOver();
        yield return null;

        // WHEN
        // A normal run ends. Reset, kill, read and reset are one step with no yield
        string trackJson;
        int? highScore;
        PlayerStats.SetDefaultPlayerStatsValues();
        try
        {
            TestUtils.KillHalalit();
            trackJson = PlayerStats._dailyTrack;
            highScore = PlayerStats._highScore;
        }
        finally
        {
            PlayerStats.SetDefaultPlayerStatsValues();
        }
        yield return null;

        // THEN
        AssertWrapper.IsTrue(highScore == shownScore, "The game over did not reach Score, so this run says nothing about the recorder", _currentSeed);
        AssertWrapper.IsTrue(trackJson == null, "A normal run handed over a track", _currentSeed);
    }

    // Until Ofir adds the recorder to the Halalit in the Playground, these tests would check nothing (Needs Ofir in Unity)
    private void AssertRecorderIsOnTheHalalit()
    {
        AssertWrapper.IsNotNull(TestUtils.GetHalalit().GetComponent<DailyTrackRecorder>(), _currentSeed, "The Halalit has no DailyTrackRecorder (Needs Ofir in Unity)");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
