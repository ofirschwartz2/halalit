using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderBoard : MonoBehaviour
{
    const string HIGHSCORE_TABLE_NAME = "HighscoreTable";
    const string LEADERBOARD_ENTRY_CONTAINER_NAME = "LeaderboardEntryContainer";
    const string LEADERBOARD_ENTRY_TEMPLATE_NAME = "LeaderboardEntryTemplate";
    const string ID_TEXT_NAME = "IdText";
    const string PLAYER_NAME_TEXT_NAME = "PlayerNameText";
    const string SCORE_TEXT_NAME = "ScoreText";

    const string COULD_NOT_LOAD_TITLE = "Could not load";

    [SerializeField]
    private TMP_Text _listTitleText;
    [SerializeField]
    private TMP_Text _placeText;
    [SerializeField]
    private Color _playerRowColor = Color.yellow;
    [SerializeField]
    private GameObject _watchButton;
    [SerializeField]
    private DailyReplay _dailyReplay;

    private Transform _entryContainer;
    private Transform _entryTemplate;
    private List<LeaderboardDay> _archivedVersions;
    private int _dayIndex;
    private bool _isAllTime;
    private bool _isLoading;

    // The list's online calls, swapped for fakes by the tests (#312). Static, because the scene's Awake can load the list
    // before a test gets control, so a test sets them before it loads the scene
    private static Func<string, Task<LeaderboardPage>> _loadPage;
    private static Func<string, Task<LeaderboardRow>> _loadPlayerRow;
    private static Func<string, int, Task<List<LeaderboardDay>>> _loadPastDays;
    private static Func<string, string, Task<LeaderboardPage>> _loadPastDayPage;
    private static Func<string, string, Task<LeaderboardRow>> _loadPastDayPlayerRow;

    // Unity may run this off the main thread while it loads the scene, so it only sets the calls: no Unity API and no log
    static LeaderBoard()
    {
        UseRealServices();
    }

    public async void Awake()
    {
        var highscoreTableContainer = Utils.FindTransformChild(transform, HIGHSCORE_TABLE_NAME);
        _entryContainer = Utils.FindTransformChild(highscoreTableContainer, LEADERBOARD_ENTRY_CONTAINER_NAME);
        _entryTemplate = Utils.FindTransformChild(_entryContainer, LEADERBOARD_ENTRY_TEMPLATE_NAME);
        _entryTemplate.gameObject.SetActive(false);

        if (_listTitleText == null)
        {
            Debug.LogWarning("LeaderBoard: _listTitleText is not wired in the Leaderboard scene, so the list title is not shown.");
        }

        if (_placeText == null)
        {
            Debug.LogWarning("LeaderBoard: _placeText is not wired in the Leaderboard scene, so your place is not shown.");
        }

        if (_watchButton == null || _dailyReplay == null)
        {
            Debug.LogWarning("LeaderBoard: _watchButton or _dailyReplay is not wired in the Leaderboard scene, so a finished day cannot be watched.");
        }

        _isLoading = true;

        // A failed load still shows today's list (#176), as "Could not load" if the sign-in failed. Before, every list button stayed dead
        try
        {
            await PlayerStats.InitAllAsync();
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"LeaderBoard: could not load your scores. {exception.Message}");
        }

        if (this == null)
        {
            return;
        }

        _isLoading = false;

        ShowToday();
    }

    private async void TryShowList(bool isAllTime, int requestedDayIndex)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            await ShowListAsync(isAllTime, requestedDayIndex);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"LeaderBoard: could not load the list. {exception.Message}");

            if (this == null)
            {
                return;
            }

            SetListTitle(COULD_NOT_LOAD_TITLE);
            SetPlaceText(string.Empty);
            SetWatchButton(false);
            DisplayScores(new List<LeaderboardRow>(), null);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task ShowListAsync(bool isAllTime, int requestedDayIndex)
    {
        if (isAllTime)
        {
            var allTimePage = await _loadPage(Constants.HIGHSCORE_LEADERBOARD_ID);
            var allTimePlayerEntry = await _loadPlayerRow(Constants.HIGHSCORE_LEADERBOARD_ID);

            if (this == null)
            {
                return;
            }

            _isAllTime = true;
            SetListTitle(DailyLeaderboard.GetListTitle(true, _dayIndex, default));
            SetPlaceText(DailyLeaderboard.GetPlaceLine(true, _dayIndex, allTimePlayerEntry?.Rank, allTimePage.Total));
            DisplayScores(allTimePage.Rows, allTimePlayerEntry);
            SetWatchButton(DailyLeaderboard.CanWatch(true, _dayIndex));
            return;
        }

        if (requestedDayIndex > 0 && _archivedVersions == null)
        {
            _archivedVersions = await _loadPastDays(Constants.DAILY_LEADERBOARD_ID, Constants.DAILY_LEADERBOARD_PAST_DAYS);
        }

        int archivedDays = _archivedVersions == null ? 0 : _archivedVersions.Count;
        int dayIndex = DailyLeaderboard.ClampDayIndex(requestedDayIndex, archivedDays);
        List<LeaderboardRow> entries;
        int playerCount;
        LeaderboardRow playerEntry;
        DateTime archivedDayStart = default;

        if (dayIndex == 0)
        {
            var todayPage = await _loadPage(Constants.DAILY_LEADERBOARD_ID);
            entries = todayPage.Rows;
            playerCount = todayPage.Total;
            playerEntry = await _loadPlayerRow(Constants.DAILY_LEADERBOARD_ID);
        }
        else
        {
            var version = _archivedVersions[dayIndex - 1];
            var versionPage = await _loadPastDayPage(Constants.DAILY_LEADERBOARD_ID, version.VersionId);
            entries = versionPage.Rows;
            playerCount = versionPage.Total;
            archivedDayStart = version.Start;
            playerEntry = await _loadPastDayPlayerRow(Constants.DAILY_LEADERBOARD_ID, version.VersionId);
        }

        if (this == null)
        {
            return;
        }

        _isAllTime = false;
        _dayIndex = dayIndex;
        SetListTitle(DailyLeaderboard.GetListTitle(false, dayIndex, archivedDayStart));
        SetPlaceText(DailyLeaderboard.GetPlaceLine(false, dayIndex, playerEntry?.Rank, playerCount));
        DisplayScores(entries, playerEntry);
        SetWatchButton(DailyLeaderboard.CanWatch(false, dayIndex));
    }

    private void SetListTitle(string title)
    {
        if (_listTitleText != null)
        {
            _listTitleText.text = title;
        }
    }

    private void SetPlaceText(string placeLine)
    {
        if (_placeText != null)
        {
            _placeText.text = placeLine;
        }
    }

    private void SetWatchButton(bool isShown)
    {
        if (_watchButton != null)
        {
            _watchButton.SetActive(isShown);
        }
    }

    public void DisplayScores(List<LeaderboardRow> entries, LeaderboardRow playerEntry)
    {
        foreach (Transform child in _entryContainer)
        {
            if (child != _entryTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        int playerIndex = playerEntry == null ? -1 : entries.FindIndex(entry => entry.PlayerId == playerEntry.PlayerId);

        for (int i = 0; i < entries.Count; i++)
        {
            AddRow(entries[i], i, i == playerIndex);
        }

        if (playerEntry != null && playerIndex == -1)
        {
            AddRow(playerEntry, DailyLeaderboard.GetRowIndexUnderShownRows(entries.Count), true);
        }
    }

    private void AddRow(LeaderboardRow entry, int rowIndex, bool isPlayer)
    {
        Transform entryTransform = Instantiate(_entryTemplate, _entryContainer);
        RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
        entryRectTransform.anchoredPosition = new Vector2(0, -70 * rowIndex);
        entryTransform.gameObject.SetActive(true);

        Utils.SetTMProTextOnComponent(entryTransform, ID_TEXT_NAME, (entry.Rank + 1).ToString());
        Utils.SetTMProTextOnComponent(entryTransform, PLAYER_NAME_TEXT_NAME, entry.PlayerName);
        Utils.SetTMProTextOnComponent(entryTransform, SCORE_TEXT_NAME, entry.Score.ToString());

        if (isPlayer)
        {
            MarkPlayerRow(entryTransform);
        }
    }

    private void MarkPlayerRow(Transform row)
    {
        foreach (string textName in new[] { ID_TEXT_NAME, PLAYER_NAME_TEXT_NAME, SCORE_TEXT_NAME })
        {
            var text = row.Find(textName).GetComponent<TMP_Text>();

            if (text != null)
            {
                text.color = _playerRowColor;
            }
        }
    }

#region Buttons
    public void ShowToday()
    {
        if (_isLoading)
        {
            return;
        }

        // Forget the archived days, so a day that ended while the screen was open shows up
        _archivedVersions = null;
        TryShowList(false, 0);
    }

    public void ShowPreviousDay()
    {
        TryShowList(false, _isAllTime ? _dayIndex : _dayIndex + 1);
    }

    public void ShowNextDay()
    {
        TryShowList(false, _isAllTime ? _dayIndex : _dayIndex - 1);
    }

    public void ShowAllTime()
    {
        TryShowList(true, _dayIndex);
    }

    // Daily replay (#244): only a finished day can be watched
    public void Watch()
    {
        if (_isLoading || !DailyLeaderboard.CanWatch(_isAllTime, _dayIndex) || _archivedVersions == null || _dailyReplay == null)
        {
            return;
        }

        var version = _archivedVersions[_dayIndex - 1];
        _dailyReplay.Open(version.VersionId, DailySeed.GetIsraelDate(version.Start));
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
#endregion

#region Services
    // _loadPage is the method itself, not a lambda, so IsListLoadReal can compare it
    private static void UseRealServices()
    {
        _loadPage = PlayerPlace.GetPageAsync;
        _loadPlayerRow = async leaderboardId => PlayerPlace.ToRow(await PlayerPlace.GetPlayerEntryAsync(leaderboardId));
        _loadPastDays = PlayerPlace.GetPastDaysAsync;
        _loadPastDayPage = PlayerPlace.GetPastDayPageAsync;
        _loadPastDayPlayerRow = async (leaderboardId, versionId) => PlayerPlace.ToRow(await PlayerPlace.GetVersionPlayerEntryAsync(leaderboardId, versionId));
    }

#if UNITY_EDITOR
    internal static void SetServices(Func<string, Task<LeaderboardPage>> loadPage, Func<string, Task<LeaderboardRow>> loadPlayerRow, Func<string, int, Task<List<LeaderboardDay>>> loadPastDays, Func<string, string, Task<LeaderboardPage>> loadPastDayPage, Func<string, string, Task<LeaderboardRow>> loadPastDayPlayerRow)
    {
        _loadPage = loadPage;
        _loadPlayerRow = loadPlayerRow;
        _loadPastDays = loadPastDays;
        _loadPastDayPage = loadPastDayPage;
        _loadPastDayPlayerRow = loadPastDayPlayerRow;
    }

    internal static void ResetServices()
    {
        UseRealServices();
    }

    // Read by the tests' guard (#312), as PlayerStats.IsSignInReal. Two delegates for the same static method compare equal
    internal static bool IsListLoadReal()
    {
        return _loadPage == (Func<string, Task<LeaderboardPage>>)PlayerPlace.GetPageAsync;
    }
#endif
#endregion

#if UNITY_EDITOR
    // Null while the field is not wired in the Leaderboard scene
    internal string GetListTitle()
    {
        return _listTitleText == null ? null : _listTitleText.text;
    }

    // Null while the field is not wired in the Leaderboard scene
    internal string GetPlaceText()
    {
        return _placeText == null ? null : _placeText.text;
    }

    // False while the button is not wired in the Leaderboard scene
    internal bool IsWatchShown()
    {
        return _watchButton != null && _watchButton.activeSelf;
    }

    internal bool IsLoading()
    {
        return _isLoading;
    }
#endif
}