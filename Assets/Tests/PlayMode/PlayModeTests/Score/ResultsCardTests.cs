using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Results card (#249): a death freezes the run on the card, which shows the score and the best, and saves once.
// Every test fakes the card's two service calls, so no test saves for real or asks Unity's servers for a place.
// The card fills itself one frame after the death, so it is read two frames after the kill, never in the kill's own frame.
public class ResultsCardTests
{
    private const string FAKE_PLACE_TEXT = "3rd of 7 so far";
    private const int MAX_FRAMES_TO_RELOAD = 10;

    private int _currentSeed;
    private int _saveCount;
    private int? _highScoreSeenBySave;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        _saveCount = 0;
        _highScoreSeenBySave = null;
    }

    [UnityTest]
    public IEnumerator DeathOpensCardWithScoreAndBest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        var silverValue = TestUtils.GetValuableValues().Find(valuableValue => valuableValue.Key == ValuableName.SILVER).Value;
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var score = TestUtils.GetScore();
        AssertWrapper.AreEqual(silverValue * 2, score, "Halalit Did Not Collect Two Silvers", _currentSeed);
        PlayerStats.SetDefaultPlayerStatsValues();

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open", _currentSeed);
        AssertWrapper.AreEqual("Score: " + score, resultsCard.GetScoreText(), "Score Text Is Not The Run's Score", _currentSeed);
        AssertWrapper.AreEqual("New best: " + score, resultsCard.GetBestText(), "Best Text Is Not A New Best Of The Run's Score", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsAgainShown(), "Again Is Hidden After A Normal Run", _currentSeed);
        AssertWrapper.IsFalse(resultsCard.IsDailyLabelShown(), "Daily Label Is Shown After A Normal Run", _currentSeed);
        AssertWrapper.AreEqual(string.Empty, resultsCard.GetPlaceText(), "Place Text Is Not Empty After A Normal Run", _currentSeed);
        AssertWrapper.AreEqual(0f, Time.timeScale, "The Run Did Not Freeze", _currentSeed);
        AssertWrapper.AreEqual(1, _saveCount, "The Card Did Not Save Exactly Once", _currentSeed);
        AssertWrapper.IsTrue(_highScoreSeenBySave == score, "The Save Ran Before Score Handed Over The Run's Score", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DailyDeathHidesAgainAndShowsPlace()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open After A Daily", _currentSeed);
        AssertWrapper.IsFalse(resultsCard.IsAgainShown(), "Again Is Shown After A Daily", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsDailyLabelShown(), "Daily Label Is Hidden After A Daily", _currentSeed);
        AssertWrapper.AreEqual(FAKE_PLACE_TEXT, resultsCard.GetPlaceText(), "Place Text Is Not Today's Place", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FailedSaveSaysPlaceCouldNotLoad()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(FailingSave);
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Closed After A Failed Save", _currentSeed);
        AssertWrapper.AreEqual("Place could not load", resultsCard.GetPlaceText(), "Place Text Does Not Say The Place Could Not Load", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AgainRestoresTimeScale()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        var testTimeScale = Time.timeScale;
        PlayerStats.SetDefaultPlayerStatsValues();
        TestUtils.KillHalalit();
        yield return null;
        yield return null;
        AssertWrapper.AreEqual(0f, Time.timeScale, "The Run Did Not Freeze Before Again", _currentSeed);

        // WHEN
        resultsCard.PlayAgain();

        // THEN
        AssertWrapper.AreEqual(testTimeScale, Time.timeScale, "Again Did Not Put The Time Scale Back", _currentSeed);

        // The old card goes with the old scene, so the tear-down runs on the reloaded Playground
        for (int i = 0; i < MAX_FRAMES_TO_RELOAD && resultsCard != null; i++)
        {
            yield return null;
        }

        AssertWrapper.IsTrue(resultsCard == null, "Again Did Not Reload The Playground", _currentSeed);
    }

    // Fails with the step to do while the Playground has no card. Both service calls are faked before the death
    private ResultsCard GetResultsCardWithFakeServices(Func<Task> saveRunScores)
    {
        var resultsCard = TestUtils.GetResultsCard();
        AssertWrapper.IsTrue(resultsCard != null, "The Playground Has No ResultsCard (Needs Ofir in Unity)", _currentSeed);
        resultsCard.SetServices(saveRunScores, LoadFakePlaceText);
        return resultsCard;
    }

    // Stands in for PlayerStats.SaveRunScoresAsync, and notes the high score that the save would send
    private Task CountingSave()
    {
        _saveCount++;
        _highScoreSeenBySave = PlayerStats._highScore;
        return Task.CompletedTask;
    }

    private Task FailingSave()
    {
        return Task.FromException(new Exception("offline"));
    }

    private Task<string> LoadFakePlaceText()
    {
        return Task.FromResult(FAKE_PLACE_TEXT);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
