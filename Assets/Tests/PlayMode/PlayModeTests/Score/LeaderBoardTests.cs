using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Leaderboard screen (#312): today's list, a past day, and Watch, all on fakes.
// The scene's Awake can load the list before a test gets control, so each test sets its own fakes first and only then loads the scene.
// PlayerStats stays on TestServices' fakes, so the scene's sign-in is fake too. Each [TearDown] puts TestServices' fakes back
public class LeaderBoardTests
{
    private const int MAX_FRAMES_TO_LOAD = 10;
    private const string LIST_LOAD_WARNING = "^LeaderBoard: could not load the list";
    private const string LIST_LOAD_FAILURE = "Fake list load failure";
    private const string PLAYER_ID = "player";
    private const string PAST_DAY_VERSION_ID = "past-day-version";
    private const string OLDER_PAST_DAY_VERSION_ID = "older-past-day-version";
    private const string REPLAY_LOAD_ERROR = "^DailyReplay: could not load the replay";
    private const string REPLAY_LOAD_FAILURE = "Fake replay load failure";

    private int _currentSeed;
    private int _loadDayCalls;
    private string _loadedDayVersionId;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        _loadDayCalls = 0;
        _loadedDayVersionId = null;
    }

    // Offline, the list says "Could not load" and logs a warning, not an error (#308). The test runner fails a test on any error,
    // so this test also proves the log is a warning
    [UnityTest]
    public IEnumerator OfflineListShowsCouldNotLoad()
    {
        // GIVEN
        LeaderBoard.SetServices(FailingLoadPage, LoadNoPlayerRow, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        LogAssert.Expect(LogType.Warning, new Regex(LIST_LOAD_WARNING));
        LeaderBoard leaderBoard = null;

        // WHEN
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);

        // THEN
        AssertWrapper.AreEqual("Could not load", leaderBoard.GetListTitle(), "The list title does not say the list could not load", _currentSeed);
        AssertWrapper.AreEqual(string.Empty, leaderBoard.GetPlaceText(), "The place line is not empty after the list failed to load", _currentSeed);
        AssertWrapper.IsFalse(leaderBoard.IsWatchShown(), "Watch is shown after the list failed to load", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TodayShowsYourPlaceSoFar()
    {
        // GIVEN
        // Three rows of seven players, and you are third
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        LeaderBoard leaderBoard = null;

        // WHEN
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);

        // THEN
        AssertWrapper.AreEqual("Today", leaderBoard.GetListTitle(), "The list title is not Today when the screen opens", _currentSeed);
        AssertWrapper.AreEqual("You: 3rd of 7 so far", leaderBoard.GetPlaceText(), "The place line is not your place so far on today's list", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PastDayShowsWatch()
    {
        // GIVEN
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        DailyReplay.SetServices(CountingLoadDay);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);
        CheckDailyReplayIsWired();

        // WHEN
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);

        // THEN
        // The past day started at noon UTC on 26 November 2026, which is the same date in Israel
        AssertWrapper.AreEqual("26-11-26", leaderBoard.GetListTitle(), "The list title is not the past day's date", _currentSeed);
        AssertWrapper.IsTrue(leaderBoard.IsWatchShown(), "Watch is hidden on a finished day. If the LeaderBoard's Watch Button is not wired, wire it (Needs Ofir in Unity)", _currentSeed);

        leaderBoard.Watch();
        yield return null;

        AssertWrapper.AreEqual(1, _loadDayCalls, "Watch on a finished day did not load that day's replay once. If the LeaderBoard's Daily Replay is not wired, wire it (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.AreEqual(PAST_DAY_VERSION_ID, _loadedDayVersionId, "Watch loaded another day than the one on screen", _currentSeed);
    }

    // Today's list is still open and all time is not one day, so neither can be watched
    [UnityTest]
    public IEnumerator TodayIsNeverWatchable()
    {
        // GIVEN
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        DailyReplay.SetServices(CountingLoadDay);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);
        CheckDailyReplayIsWired();
        AssertWrapper.IsFalse(leaderBoard.IsWatchShown(), "Watch is shown on today's list when the screen opens", _currentSeed);
        // Shown on the past day, so the checks below cannot pass only because Watch is not wired
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.IsTrue(leaderBoard.IsWatchShown(), "Watch is hidden on a finished day. If the LeaderBoard's Watch Button is not wired, wire it (Needs Ofir in Unity)", _currentSeed);

        // WHEN
        leaderBoard.ShowNextDay();
        yield return WaitForTheLoadToEnd(leaderBoard);

        // THEN
        AssertWrapper.AreEqual("Today", leaderBoard.GetListTitle(), "Next day from the past day did not go back to today", _currentSeed);
        AssertWrapper.IsFalse(leaderBoard.IsWatchShown(), "Watch is shown again on today's list", _currentSeed);
        leaderBoard.Watch();
        yield return null;
        AssertWrapper.AreEqual(0, _loadDayCalls, "Watch on today's list loaded a replay", _currentSeed);

        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        leaderBoard.ShowAllTime();
        yield return WaitForTheLoadToEnd(leaderBoard);

        AssertWrapper.AreEqual("All time", leaderBoard.GetListTitle(), "All time did not open the all-time list", _currentSeed);
        AssertWrapper.IsFalse(leaderBoard.IsWatchShown(), "Watch is shown on the all-time list", _currentSeed);
        leaderBoard.Watch();
        yield return null;
        AssertWrapper.AreEqual(0, _loadDayCalls, "Watch on the all-time list loaded a replay", _currentSeed);

        // Watch does load on the past day, so the two counts of 0 above cannot pass only because the LeaderBoard's Daily Replay is not wired
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        leaderBoard.Watch();
        yield return null;
        AssertWrapper.AreEqual(1, _loadDayCalls, "Watch on a finished day did not load its replay. If the LeaderBoard's Daily Replay is not wired, wire it (Needs Ofir in Unity)", _currentSeed);
    }

    // Previous goes back one finished day at a time and stops at the oldest; Next comes forward and stops at today
    [UnityTest]
    public IEnumerator PreviousAndNextDayStopAtTheEnds()
    {
        // GIVEN
        // Two finished days, newest first: 26 and 25 November 2026
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadTwoPastDays, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);

        // WHEN, THEN
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("26-11-26", leaderBoard.GetListTitle(), "Previous day from today did not open the newest finished day", _currentSeed);

        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("25-11-26", leaderBoard.GetListTitle(), "Previous day did not open the older finished day", _currentSeed);

        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("25-11-26", leaderBoard.GetListTitle(), "Previous day on the oldest finished day did not stay on it", _currentSeed);

        leaderBoard.ShowNextDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("26-11-26", leaderBoard.GetListTitle(), "Next day from the oldest finished day did not open the newer one", _currentSeed);

        leaderBoard.ShowNextDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("Today", leaderBoard.GetListTitle(), "Next day from the newest finished day did not go back to today", _currentSeed);

        leaderBoard.ShowNextDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.AreEqual("Today", leaderBoard.GetListTitle(), "Next day on today did not stay on today", _currentSeed);
        AssertWrapper.AreEqual("You: 3rd of 7 so far", leaderBoard.GetPlaceText(), "Back on today, the place line is not your place so far", _currentSeed);
    }

    // All time reads the all-time list, not today's, and its place has no "so far"
    [UnityTest]
    public IEnumerator AllTimeShowsYourBestPlace()
    {
        // GIVEN
        // Today you are 3rd of 7; on the all-time list you are 1st of 12
        LeaderBoard.SetServices(LoadPageByList, LoadPlayerRowByList, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);
        AssertWrapper.AreEqual("You: 3rd of 7 so far", leaderBoard.GetPlaceText(), "The place line is not your place so far on today's list", _currentSeed);

        // WHEN
        leaderBoard.ShowAllTime();
        yield return WaitForTheLoadToEnd(leaderBoard);

        // THEN
        AssertWrapper.AreEqual("All time", leaderBoard.GetListTitle(), "All time did not open the all-time list", _currentSeed);
        AssertWrapper.AreEqual("You: 1st of 12", leaderBoard.GetPlaceText(), "The place line is not your place on the all-time list", _currentSeed);
    }

    [UnityTest]
    public IEnumerator PastDayWithoutYourScoreSaysYouDidNotPlay()
    {
        // GIVEN
        // Two other players finished the past day, and you have no score on it
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadOnePastDay, LoadPastDayPageWithoutYou, LoadNoPastDayPlayerRow);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);

        // WHEN
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);

        // THEN
        AssertWrapper.AreEqual("26-11-26", leaderBoard.GetListTitle(), "The list title is not the past day's date", _currentSeed);
        AssertWrapper.AreEqual("You did not play", leaderBoard.GetPlaceText(), "The place line on a past day without your score does not say you did not play", _currentSeed);
    }

    // A failed load hides a Watch that was shown, so the screen never offers a replay under "Could not load" (#308: logged as a warning)
    [UnityTest]
    public IEnumerator FailedPastDayLoadHidesWatch()
    {
        // GIVEN
        // The newer finished day loads; the older one fails
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadTwoPastDays, FailingOlderPastDayPage, LoadNoPastDayPlayerRow);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.IsTrue(leaderBoard.IsWatchShown(), "Watch is hidden on a finished day. If the LeaderBoard's Watch Button is not wired, wire it (Needs Ofir in Unity)", _currentSeed);
        LogAssert.Expect(LogType.Warning, new Regex(LIST_LOAD_WARNING));

        // WHEN
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);

        // THEN
        AssertWrapper.AreEqual("Could not load", leaderBoard.GetListTitle(), "The list title does not say the past day could not load", _currentSeed);
        AssertWrapper.AreEqual(string.Empty, leaderBoard.GetPlaceText(), "The place line is not empty after the past day failed to load", _currentSeed);
        AssertWrapper.IsFalse(leaderBoard.IsWatchShown(), "Watch is still shown after the past day failed to load", _currentSeed);
    }

    // Offline, Watch logs the failure and stays usable: a second tap tries again.
    // DailyReplay.TryLoad logs this as an error today. Card #314: make it a warning, as #308 did for the list. That change must update this expectation.
    // The "Could not load" status line is not checked: the test assembly cannot read DailyReplay's text without a test hook
    [UnityTest]
    public IEnumerator OfflineReplayCanBeTriedAgain()
    {
        // GIVEN
        LeaderBoard.SetServices(LoadTodayPage, LoadThirdPlayerRow, LoadOnePastDay, LoadEmptyPastDayPage, LoadNoPastDayPlayerRow);
        DailyReplay.SetServices(FailingCountingLoadDay);
        LeaderBoard leaderBoard = null;
        yield return LoadLeaderBoard(loaded => leaderBoard = loaded);
        CheckDailyReplayIsWired();
        leaderBoard.ShowPreviousDay();
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.IsTrue(leaderBoard.IsWatchShown(), "Watch is hidden on a finished day. If the LeaderBoard's Watch Button is not wired, wire it (Needs Ofir in Unity)", _currentSeed);
        LogAssert.Expect(LogType.Error, new Regex(REPLAY_LOAD_ERROR));
        LogAssert.Expect(LogType.Error, new Regex(REPLAY_LOAD_ERROR));

        // WHEN
        leaderBoard.Watch();
        yield return null;
        AssertWrapper.AreEqual(1, _loadDayCalls, "Watch on a finished day did not try to load its replay. If the LeaderBoard's Daily Replay is not wired, wire it (Needs Ofir in Unity)", _currentSeed);
        leaderBoard.Watch();
        yield return null;

        // THEN
        AssertWrapper.AreEqual(2, _loadDayCalls, "After a failed replay load, a second Watch did not try again", _currentSeed);
        AssertWrapper.AreEqual("26-11-26", leaderBoard.GetListTitle(), "A failed replay load changed the list on screen", _currentSeed);
    }

    #region Load and waits
    // Loads the scene after the test has set its fakes, waits for the first list, and stops with the step to do while a field is not wired
    private IEnumerator LoadLeaderBoard(Action<LeaderBoard> onLoaded)
    {
        SceneManager.LoadScene(SceneName.LEADERBOARD.GetDescription());
        LeaderBoard leaderBoard = null;

        for (int i = 0; i < MAX_FRAMES_TO_LOAD && leaderBoard == null; i++)
        {
            yield return null;
            leaderBoard = UnityEngine.Object.FindObjectOfType<LeaderBoard>();
        }

        AssertWrapper.IsNotNull(leaderBoard, _currentSeed, "The Leaderboard scene has no LeaderBoard");
        yield return WaitForTheLoadToEnd(leaderBoard);
        AssertWrapper.IsFalse(leaderBoard.IsLoading(), "The leaderboard did not finish loading, though every fake had finished", _currentSeed);
        AssertWrapper.IsTrue(leaderBoard.GetListTitle() != null, "The LeaderBoard's List Title Text is not wired in the Leaderboard scene (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.IsTrue(leaderBoard.GetPlaceText() != null, "The LeaderBoard's Place Text is not wired in the Leaderboard scene (Needs Ofir in Unity)", _currentSeed);
        onLoaded(leaderBoard);
    }

    // A finished task's continuation can run on Unity's synchronization context, a frame or two later
    private IEnumerator WaitForTheLoadToEnd(LeaderBoard leaderBoard)
    {
        for (int i = 0; i < MAX_FRAMES_TO_LOAD && leaderBoard != null && leaderBoard.IsLoading(); i++)
        {
            yield return null;
        }
    }

    // A DailyReplay with an empty field switches itself off, and then Watch loads nothing.
    // One on an inactive object never runs Awake, so it stays enabled but is not ready, and Watch loads nothing either
    private void CheckDailyReplayIsWired()
    {
        var dailyReplay = UnityEngine.Object.FindObjectOfType<DailyReplay>(true);
        AssertWrapper.IsTrue(dailyReplay != null, "The Leaderboard scene has no DailyReplay (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.IsTrue(dailyReplay.isActiveAndEnabled, "The Leaderboard scene's DailyReplay switched itself off because a field is not wired, or sits on an inactive object. Put it on an active object and wire its fields (Needs Ofir in Unity)", _currentSeed);
    }
    #endregion

    #region Fakes
    private static LeaderboardRow NewRow(string playerId, int rank, double score)
    {
        return new LeaderboardRow { PlayerId = playerId, PlayerName = playerId + "Name", Rank = rank, Score = score };
    }

    private Task<LeaderboardPage> FailingLoadPage(string leaderboardId)
    {
        return Task.FromException<LeaderboardPage>(new Exception(LIST_LOAD_FAILURE));
    }

    // Unity's Rank starts at 0, so rank 2 is third place
    private Task<LeaderboardPage> LoadTodayPage(string leaderboardId)
    {
        var rows = new List<LeaderboardRow> { NewRow("first", 0, 30), NewRow("second", 1, 20), NewRow(PLAYER_ID, 2, 10) };
        return Task.FromResult(new LeaderboardPage { Rows = rows, Total = 7 });
    }

    private Task<LeaderboardRow> LoadThirdPlayerRow(string leaderboardId)
    {
        return Task.FromResult(NewRow(PLAYER_ID, 2, 10));
    }

    private Task<LeaderboardRow> LoadNoPlayerRow(string leaderboardId)
    {
        return Task.FromResult<LeaderboardRow>(null);
    }

    private Task<List<LeaderboardDay>> LoadOnePastDay(string leaderboardId, int limit)
    {
        var pastDay = new LeaderboardDay { VersionId = PAST_DAY_VERSION_ID, Start = new DateTime(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc) };
        return Task.FromResult(new List<LeaderboardDay> { pastDay });
    }

    // Newest first, as Unity sends them
    private Task<List<LeaderboardDay>> LoadTwoPastDays(string leaderboardId, int limit)
    {
        var pastDay = new LeaderboardDay { VersionId = PAST_DAY_VERSION_ID, Start = new DateTime(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc) };
        var olderPastDay = new LeaderboardDay { VersionId = OLDER_PAST_DAY_VERSION_ID, Start = new DateTime(2026, 11, 25, 12, 0, 0, DateTimeKind.Utc) };
        return Task.FromResult(new List<LeaderboardDay> { pastDay, olderPastDay });
    }

    // On the all-time list you are first of 12; on today's list, third of 7
    private Task<LeaderboardPage> LoadPageByList(string leaderboardId)
    {
        if (leaderboardId != Constants.HIGHSCORE_LEADERBOARD_ID)
        {
            return LoadTodayPage(leaderboardId);
        }

        var rows = new List<LeaderboardRow> { NewRow(PLAYER_ID, 0, 500), NewRow("second", 1, 400) };
        return Task.FromResult(new LeaderboardPage { Rows = rows, Total = 12 });
    }

    private Task<LeaderboardRow> LoadPlayerRowByList(string leaderboardId)
    {
        if (leaderboardId != Constants.HIGHSCORE_LEADERBOARD_ID)
        {
            return LoadThirdPlayerRow(leaderboardId);
        }

        return Task.FromResult(NewRow(PLAYER_ID, 0, 500));
    }

    private Task<LeaderboardPage> LoadEmptyPastDayPage(string leaderboardId, string versionId)
    {
        return Task.FromResult(new LeaderboardPage { Rows = new List<LeaderboardRow>(), Total = 0 });
    }

    private Task<LeaderboardPage> LoadPastDayPageWithoutYou(string leaderboardId, string versionId)
    {
        var rows = new List<LeaderboardRow> { NewRow("first", 0, 30), NewRow("second", 1, 20) };
        return Task.FromResult(new LeaderboardPage { Rows = rows, Total = 2 });
    }

    private Task<LeaderboardPage> FailingOlderPastDayPage(string leaderboardId, string versionId)
    {
        if (versionId == OLDER_PAST_DAY_VERSION_ID)
        {
            return Task.FromException<LeaderboardPage>(new Exception(LIST_LOAD_FAILURE));
        }

        return LoadEmptyPastDayPage(leaderboardId, versionId);
    }

    private Task<LeaderboardRow> LoadNoPastDayPlayerRow(string leaderboardId, string versionId)
    {
        return Task.FromResult<LeaderboardRow>(null);
    }

    private Task<List<DailyReplayEntry>> CountingLoadDay(string versionId, DateTime israelDay)
    {
        _loadDayCalls++;
        _loadedDayVersionId = versionId;
        return Task.FromResult(new List<DailyReplayEntry>());
    }

    private Task<List<DailyReplayEntry>> FailingCountingLoadDay(string versionId, DateTime israelDay)
    {
        _loadDayCalls++;
        _loadedDayVersionId = versionId;
        return Task.FromException<List<DailyReplayEntry>>(new Exception(REPLAY_LOAD_FAILURE));
    }
    #endregion

    // Nothing here sets a time scale, so ResetTimeScale is not called: it would put back a saved 0
    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestServices.UseFakes();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
