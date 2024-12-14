using Assets.Enums;
using Assets.Utils;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderBoard : MonoBehaviour
{
    const string HIGHSCORE_LEADERBOARD_ID = "1";
    const string HIGHSCORE_TABLE_NAME = "HighscoreTable";
    const string LEADERBOARD_ENTRY_CONTAINER_NAME = "LeaderboardEntryContainer";
    const string LEADERBOARD_ENTRY_TEMPLATE_NAME = "LeaderboardEntryTemplate";
    const string ID_TEXT_NAME = "IdText";
    const string PLAYER_NAME_TEXT_NAME = "PlayerNameText";
    const string SCORE_TEXT_NAME = "ScoreText";

    private LeaderboardScoresPage _highScoresResponse;

    public async void Awake()
    {
        await PlayerStats.InitAllAsync();

        await TryGetScores();

        DisplayScores();
    }

    public async Task TryGetScores()
    {
        _highScoresResponse = await LeaderboardsService.Instance.GetScoresAsync(HIGHSCORE_LEADERBOARD_ID);
    }

    public void DisplayScores()
    {
        var highscoreTableContainer = Utils.FindTransformChild(transform, HIGHSCORE_TABLE_NAME);
        var entryContainer = Utils.FindTransformChild(highscoreTableContainer, LEADERBOARD_ENTRY_CONTAINER_NAME);
        var entryTemplate = Utils.FindTransformChild(entryContainer, LEADERBOARD_ENTRY_TEMPLATE_NAME);


        for (int i = 0; i < _highScoresResponse.Results.Count; i++)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -70 * i);
            entryTransform.gameObject.SetActive(true);

            Utils.SetTMProTextOnComponent(entryTransform, ID_TEXT_NAME, (i + 1).ToString());
            Utils.SetTMProTextOnComponent(entryTransform, PLAYER_NAME_TEXT_NAME, _highScoresResponse.Results[i].PlayerName);
            Utils.SetTMProTextOnComponent(entryTransform, SCORE_TEXT_NAME, _highScoresResponse.Results[i].Score.ToString());
        }

        entryTemplate.gameObject.SetActive(false);

    }

#region Buttons
    public void BackToMainMenu()
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
#endregion

}
