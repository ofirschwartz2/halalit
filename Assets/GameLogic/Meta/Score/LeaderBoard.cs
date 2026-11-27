using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
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

    private Transform _entryContainer;
    private Transform _entryTemplate;
    private List<LeaderboardVersion> _archivedVersions;
    private int _dayIndex;
    private bool _isAllTime;
    private bool _isLoading;

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

        _isLoading = true;
        await PlayerStats.InitAllAsync();
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
            Debug.LogError($"LeaderBoard: could not load the list. {exception.Message}");

            if (this == null)
            {
                return;
            }

            SetListTitle(COULD_NOT_LOAD_TITLE);
            SetPlaceText(string.Empty);
            DisplayScores(new List<LeaderboardEntry>(), null);
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
            var allTimePage = await LeaderboardsService.Instance.GetScoresAsync(Constants.HIGHSCORE_LEADERBOARD_ID);
            var allTimePlayerEntry = await PlayerPlace.GetPlayerEntryAsync(Constants.HIGHSCORE_LEADERBOARD_ID);

            if (this == null)
            {
                return;
            }

            _isAllTime = true;
            SetListTitle(DailyLeaderboard.GetListTitle(true, _dayIndex, default));
            SetPlaceText(DailyLeaderboard.GetPlaceLine(true, _dayIndex, allTimePlayerEntry?.Rank, allTimePage.Total));
            DisplayScores(allTimePage.Results, allTimePlayerEntry);
            return;
        }

        if (requestedDayIndex > 0 && _archivedVersions == null)
        {
            var versions = await LeaderboardsService.Instance.GetVersionsAsync(Constants.DAILY_LEADERBOARD_ID, new GetVersionsOptions { Limit = Constants.DAILY_LEADERBOARD_PAST_DAYS });
            _archivedVersions = versions.Results;
        }

        int archivedDays = _archivedVersions == null ? 0 : _archivedVersions.Count;
        int dayIndex = DailyLeaderboard.ClampDayIndex(requestedDayIndex, archivedDays);
        List<LeaderboardEntry> entries;
        int playerCount;
        LeaderboardEntry playerEntry;
        DateTime archivedDayStart = default;

        if (dayIndex == 0)
        {
            var todayPage = await LeaderboardsService.Instance.GetScoresAsync(Constants.DAILY_LEADERBOARD_ID);
            entries = todayPage.Results;
            playerCount = todayPage.Total;
            playerEntry = await PlayerPlace.GetPlayerEntryAsync(Constants.DAILY_LEADERBOARD_ID);
        }
        else
        {
            var version = _archivedVersions[dayIndex - 1];
            var versionPage = await LeaderboardsService.Instance.GetVersionScoresAsync(Constants.DAILY_LEADERBOARD_ID, version.Id);
            entries = versionPage.Results;
            playerCount = versionPage.Total;
            archivedDayStart = version.Start;
            playerEntry = await PlayerPlace.GetVersionPlayerEntryAsync(Constants.DAILY_LEADERBOARD_ID, version.Id);
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

    public void DisplayScores(List<LeaderboardEntry> entries, LeaderboardEntry playerEntry)
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
            AddRow(playerEntry, DailyLeaderboard.GetPlayerRowIndex(playerIndex, entries.Count), true);
        }
    }

    private void AddRow(LeaderboardEntry entry, int rowIndex, bool isPlayer)
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

    public void BackToMainMenu()
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
#endregion

}