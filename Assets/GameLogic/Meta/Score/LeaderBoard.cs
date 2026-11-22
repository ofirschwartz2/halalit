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
            SetListTitle(COULD_NOT_LOAD_TITLE);
            DisplayScores(new List<LeaderboardEntry>());
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
            _isAllTime = true;
            SetListTitle(DailyLeaderboard.GetListTitle(true, _dayIndex, default));
            DisplayScores(allTimePage.Results);
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
        DateTime archivedDayStart = default;

        if (dayIndex == 0)
        {
            var todayPage = await LeaderboardsService.Instance.GetScoresAsync(Constants.DAILY_LEADERBOARD_ID);
            entries = todayPage.Results;
        }
        else
        {
            var version = _archivedVersions[dayIndex - 1];
            var versionPage = await LeaderboardsService.Instance.GetVersionScoresAsync(Constants.DAILY_LEADERBOARD_ID, version.Id);
            entries = versionPage.Results;
            archivedDayStart = version.Start;
        }

        _isAllTime = false;
        _dayIndex = dayIndex;
        SetListTitle(DailyLeaderboard.GetListTitle(false, dayIndex, archivedDayStart));
        DisplayScores(entries);
    }

    private void SetListTitle(string title)
    {
        if (_listTitleText != null)
        {
            _listTitleText.text = title;
        }
    }

    public void DisplayScores(List<LeaderboardEntry> entries)
    {
        foreach (Transform child in _entryContainer)
        {
            if (child != _entryTemplate)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < entries.Count; i++)
        {
            Transform entryTransform = Instantiate(_entryTemplate, _entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -70 * i);
            entryTransform.gameObject.SetActive(true);

            Utils.SetTMProTextOnComponent(entryTransform, ID_TEXT_NAME, (i + 1).ToString());
            Utils.SetTMProTextOnComponent(entryTransform, PLAYER_NAME_TEXT_NAME, entries[i].PlayerName);
            Utils.SetTMProTextOnComponent(entryTransform, SCORE_TEXT_NAME, entries[i].Score.ToString());
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