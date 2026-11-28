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
        var halalitMovement = TestUtils.GetHalalitMovement();

        // WHEN
        // The ship flies right for a while and collects a valuable
        var moveEndTime = Time.time + MOVE_SECONDS;
        while (Time.time < moveEndTime)
        {
            halalitMovement.TryMove(1f, 0f, Time.deltaTime);
            yield return null;
        }

        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var shownScore = TestUtils.GetScore();

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
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();
        try
        {
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
    }

    [UnityTest]
    public IEnumerator NormalRunHandsOverNoTrack()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return new WaitForSeconds(1);

        TestUtils.UnhookResultsCardFromGameOver();
        yield return null;

        // WHEN
        // A normal run ends. Reset, kill, read and reset are one step with no yield
        string trackJson;
        PlayerStats.SetDefaultPlayerStatsValues();
        try
        {
            TestUtils.KillHalalit();
            trackJson = PlayerStats._dailyTrack;
        }
        finally
        {
            PlayerStats.SetDefaultPlayerStatsValues();
        }
        yield return null;

        // THEN
        AssertWrapper.IsTrue(trackJson == null, "A normal run handed over a track", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
