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
    private const int BEST_ABOVE_SCORE = 100;
    private const float NEWER_TIME_SCALE = 2f; // neither the test's 5, the usual 1 nor the card's 0
    private const string UNWIRED_RESULTS_CARD_NAME = "UnwiredResultsCard";

    private int _currentSeed;
    private int _saveCount;
    private int? _highScoreSeenBySave;
    private int? _dailyScoreSeenBySave;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        _saveCount = 0;
        _highScoreSeenBySave = null;
        _dailyScoreSeenBySave = null;
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
        AssertWrapper.IsFalse(resultsCard.IsPanelShown(), "The Results Card Is On Screen Before The Death", _currentSeed);

        // WHEN
        TestUtils.KillHalalit();
        // Nothing pins whether Score or GameOver hears the death first, so a save in the death's own frame could miss the score
        var saveCountInTheDeathFrame = _saveCount;
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsPanelShown(), "The Results Card Is Not On Screen After The Death", _currentSeed);
        AssertWrapper.AreEqual("Score: " + score, resultsCard.GetScoreText(), "Score Text Is Not The Run's Score", _currentSeed);
        AssertWrapper.AreEqual("New best: " + score, resultsCard.GetBestText(), "Best Text Is Not A New Best Of The Run's Score", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsAgainShown(), "Again Is Hidden After A Normal Run", _currentSeed);
        AssertWrapper.IsFalse(resultsCard.IsDailyLabelShown(), "Daily Label Is Shown After A Normal Run", _currentSeed);
        AssertWrapper.AreEqual(string.Empty, resultsCard.GetPlaceText(), "Place Text Is Not Empty After A Normal Run", _currentSeed);
        AssertWrapper.AreEqual(0f, Time.timeScale, "The Run Did Not Freeze", _currentSeed);
        AssertWrapper.AreEqual(0, saveCountInTheDeathFrame, "The Card Saved In The Death's Own Frame, Before Score Is Sure To Have Handed Over The Score", _currentSeed);
        AssertWrapper.AreEqual(1, _saveCount, "The Card Did Not Save Exactly Once", _currentSeed);
        AssertWrapper.IsTrue(_highScoreSeenBySave == score, "The Save Ran Before Score Handed Over The Run's Score", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DeathWithoutNewBestShowsBest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var score = TestUtils.GetScore();
        AssertWrapper.Greater(score, 0, "Halalit Did Not Collect A Silver", _currentSeed);
        // A best loaded by the menu, above this run's score
        PlayerStats.SetDefaultPlayerStatsValues();
        var savedBest = score + BEST_ABOVE_SCORE;
        PlayerStats._highScore = savedBest;

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open", _currentSeed);
        AssertWrapper.AreEqual("Score: " + score, resultsCard.GetScoreText(), "Score Text Is Not The Run's Score", _currentSeed);
        AssertWrapper.AreEqual("Best: " + savedBest, resultsCard.GetBestText(), "Best Text Is Not The Saved Best When The Run Is Not A New Best", _currentSeed);
    }

    // The new-best flag means a best is waiting to be saved. It stays on while an earlier save runs, or after one failed offline,
    // so a lower run after Again must say "Best: N", not "New best" with the earlier run's best
    [UnityTest]
    public IEnumerator LowerRunAfterAnUnsavedBestShowsBest()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        var score = TestUtils.GetScore();
        AssertWrapper.Greater(score, 0, "Halalit Did Not Collect A Silver", _currentSeed);
        // An earlier run's best, above this run's score, that is not saved yet
        PlayerStats.SetDefaultPlayerStatsValues();
        var unsavedBest = score + BEST_ABOVE_SCORE;
        PlayerStats._highScore = unsavedBest;
        PlayerStats._isNewHighScore = true;

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open", _currentSeed);
        AssertWrapper.AreEqual("Score: " + score, resultsCard.GetScoreText(), "Score Text Is Not The Run's Score", _currentSeed);
        AssertWrapper.AreEqual("Best: " + unsavedBest, resultsCard.GetBestText(), "A Run Below An Unsaved Best Says New Best", _currentSeed);
    }

    // The save is faked as a real save that worked, which switches the daily off, so Again and the place are read after that
    [UnityTest]
    public IEnumerator DailyDeathHidesAgainAndShowsPlace()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(SaveLikeTheGame);
        // 0 here. The daily score stays null until Score hands it over
        var score = TestUtils.GetScore();
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();

        // WHEN
        TestUtils.KillHalalit();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open After A Daily", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsPanelShown(), "The Results Card Is Not On Screen After A Daily", _currentSeed);
        AssertWrapper.IsFalse(resultsCard.IsAgainShown(), "Again Is Shown After A Daily", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsDailyLabelShown(), "Daily Label Is Hidden After A Daily", _currentSeed);
        AssertWrapper.AreEqual(FAKE_PLACE_TEXT, resultsCard.GetPlaceText(), "Place Text Is Not Today's Place", _currentSeed);
        AssertWrapper.IsTrue(_dailyScoreSeenBySave == score, "The Save Ran Before Score Handed Over The Daily Score", _currentSeed);
        AssertWrapper.IsFalse(PlayerStats._isDailyRun, "The Fake Save Did Not Switch The Daily Off As The Real Save Does, So Again And The Place Were Not Read After It", _currentSeed);
    }

    // The card shows at once and does not wait for the save, which can take seconds on a slow network
    [UnityTest]
    public IEnumerator DailyCardSaysSavingUntilTheSaveEnds()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var pendingSave = new TaskCompletionSource<bool>();
        var resultsCard = GetResultsCardWithFakeServices(() => pendingSave.Task);
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();
        TestUtils.KillHalalit();
        yield return null;
        yield return null;
        AssertWrapper.IsTrue(resultsCard.IsOpen(), "Results Card Did Not Open While The Save Runs", _currentSeed);
        AssertWrapper.IsTrue(resultsCard.IsPanelShown(), "The Results Card Is Not On Screen While The Save Runs", _currentSeed);
        AssertWrapper.AreEqual("Saving...", resultsCard.GetPlaceText(), "Place Text Does Not Say Saving While The Save Runs", _currentSeed);

        // WHEN
        pendingSave.SetResult(true);
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.AreEqual(FAKE_PLACE_TEXT, resultsCard.GetPlaceText(), "Place Text Is Not Today's Place After The Save Ended", _currentSeed);
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
        AssertWrapper.IsTrue(resultsCard.IsPanelShown(), "Results Card Closed After A Failed Save", _currentSeed);
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
        yield return WaitForTheOldCardToGo(resultsCard);

        AssertWrapper.IsTrue(resultsCard == null, "Again Did Not Reload The Playground", _currentSeed);
        AssertWrapper.AreEqual(TestUtils.PLAYGROUND_SCENE_NAME, SceneManager.GetActiveScene().name, "Again Loaded Another Scene Than The Playground", _currentSeed);
        AssertWrapper.AreEqual(testTimeScale, Time.timeScale, "The New Run Started Frozen", _currentSeed);
    }

    // GameOver opens the card once per death. A second Open must change nothing: if it remembered the frozen 0, Again would start the next run frozen
    [UnityTest]
    public IEnumerator SecondOpenIsIgnored()
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
        AssertWrapper.AreEqual(1, _saveCount, "The Card Did Not Save Once Before The Second Open", _currentSeed);

        // WHEN
        resultsCard.Open();
        yield return null;
        yield return null;
        var saveCountAfterTheSecondOpen = _saveCount;
        resultsCard.PlayAgain();
        var timeScaleAfterAgain = Time.timeScale;
        yield return WaitForTheOldCardToGo(resultsCard);

        // THEN
        AssertWrapper.AreEqual(1, saveCountAfterTheSecondOpen, "A Second Open Saved Again", _currentSeed);
        AssertWrapper.AreEqual(testTimeScale, timeScaleAfterAgain, "After A Second Open, Again Did Not Put The Run's Time Scale Back, So The Next Run Would Start Frozen", _currentSeed);
    }

    // Any way out but the buttons, such as a scene change or a test's tear-down, must not leave time stopped
    [UnityTest]
    public IEnumerator DestroyedOpenCardPutsTimeScaleBack()
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
        AssertWrapper.AreEqual(0f, Time.timeScale, "The Run Did Not Freeze Before The Card Was Destroyed", _currentSeed);

        // WHEN
        UnityEngine.Object.Destroy(resultsCard.gameObject);
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard == null, "The Card Was Not Destroyed", _currentSeed);
        AssertWrapper.AreEqual(testTimeScale, Time.timeScale, "A Destroyed Open Card Left Time Stopped", _currentSeed);
    }

    // As in a tear-down, where ResetTimeScale runs before the end-of-frame destroy: the card must not put the test's 5 back over a newer time scale
    [UnityTest]
    public IEnumerator DestroyedOpenCardKeepsANewerTimeScale()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var resultsCard = GetResultsCardWithFakeServices(CountingSave);
        PlayerStats.SetDefaultPlayerStatsValues();
        TestUtils.KillHalalit();
        yield return null;
        yield return null;
        AssertWrapper.AreEqual(0f, Time.timeScale, "The Run Did Not Freeze Before The Time Scale Was Changed", _currentSeed);

        // WHEN
        TestTimeController.SetCustomTimeScale(NEWER_TIME_SCALE);
        UnityEngine.Object.Destroy(resultsCard.gameObject);
        yield return null;

        // THEN
        AssertWrapper.IsTrue(resultsCard == null, "The Card Was Not Destroyed", _currentSeed);
        AssertWrapper.AreEqual(NEWER_TIME_SCALE, Time.timeScale, "A Destroyed Open Card Overwrote A Time Scale Set After It Opened", _currentSeed);
    }

    // A card with an empty field switches itself off, so GameOver loads the menu instead of freezing the run with nothing on screen.
    // This test adds its own card with no field wired, so it does not need the Playground's card
    [UnityTest]
    public IEnumerator UnwiredCardSwitchesItselfOff()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;

        // WHEN
        var unwiredResultsCard = new GameObject(UNWIRED_RESULTS_CARD_NAME).AddComponent<ResultsCard>();
        yield return null;

        // THEN
        AssertWrapper.IsFalse(unwiredResultsCard.isActiveAndEnabled, "A ResultsCard With Empty Fields Stayed On, So GameOver Would Open It", _currentSeed);
    }

    // A card that is not open stored no time scale. If its buttons answered, they would put back that empty 0 and stop time in the next scene.
    // This test adds its own card with no field wired, so it does not need the Playground's card
    [UnityTest]
    public IEnumerator ButtonOnACardThatIsNotOpenDoesNothing()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        var unwiredResultsCard = new GameObject(UNWIRED_RESULTS_CARD_NAME).AddComponent<ResultsCard>();
        yield return null;
        var timeScaleBefore = Time.timeScale;

        // WHEN
        unwiredResultsCard.PlayAgain();
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.AreEqual(timeScaleBefore, Time.timeScale, "A Button On A Card That Is Not Open Changed The Time Scale", _currentSeed);
        AssertWrapper.IsTrue(unwiredResultsCard != null, "A Button On A Card That Is Not Open Loaded A Scene", _currentSeed);
    }

    // Fails with the step to do while the Playground has no card, or has one with an empty field. Both service calls are faked before the death
    private ResultsCard GetResultsCardWithFakeServices(Func<Task> saveRunScores)
    {
        var resultsCard = TestUtils.GetResultsCard();
        AssertWrapper.IsTrue(resultsCard != null, "The Playground Has No ResultsCard (Needs Ofir in Unity)", _currentSeed);
        // GameOver loads the real menu, which signs in for real, when the card is off, and a card with an empty field switches itself off.
        // So the test stops here, before the death
        AssertWrapper.IsTrue(resultsCard.isActiveAndEnabled, "The Playground's ResultsCard Switched Itself Off Because A Field Is Not Wired (Needs Ofir in Unity)", _currentSeed);
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

    // Stands in for a PlayerStats.SaveRunScoresAsync that worked: notes the scores it would send, then clears the flags as the real save does.
    // The real save switches the daily off, so a card that read the daily flag again after the save would skip the place
    private Task SaveLikeTheGame()
    {
        _saveCount++;
        _highScoreSeenBySave = PlayerStats._highScore;
        _dailyScoreSeenBySave = PlayerStats._dailyScore;
        PlayerStats._isNewHighScore = false;
        PlayerStats._isDailyRun = false;
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

    // Again loads the Playground again. The old card goes with the old scene
    private IEnumerator WaitForTheOldCardToGo(ResultsCard oldResultsCard)
    {
        for (int i = 0; i < MAX_FRAMES_TO_RELOAD && oldResultsCard != null; i++)
        {
            yield return null;
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
