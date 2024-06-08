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
    private LeaderboardScoresPage _highScoresResponse;

    private Transform entryContainer;
    private Transform entryTemplate;

    public async void Awake()
    {
        await PlayerInitialization();

        await GetScores();

        DisplayScores();
    }

    private static async Task PlayerInitialization() // TODO: FIX
    {
        if (Authentication.InitializationTask == null)
        {
            await Authentication.Initialize();
        }
        else
        {
            await Authentication.InitializationTask;
        }

        await PlayerStats.InitAllAsync();
    }

    public async Task GetScores()
    {
        _highScoresResponse = await LeaderboardsService.Instance.GetScoresAsync(HIGHSCORE_LEADERBOARD_ID);
        _highScoresResponse.Results.ForEach(score =>
        {
            Debug.Log($"Player: {score.PlayerId}, PlayerName: {score.PlayerName}, Score: {score.Score}");
        });

        Debug.Log("Leaderboard Scores:" + _highScoresResponse.ToString());

    }

    public void DisplayScores() 
    {
        entryContainer = transform.Find("LeaderboardEntryContainer");
        entryTemplate = entryContainer.transform.Find("LeaderboardEntryTemplate");

        for (int i = 0; i < _highScoresResponse.Results.Count; i++)
        {
            Transform entryTransform = Instantiate(entryTemplate, entryContainer);
            RectTransform entryRectTransform = entryTransform.GetComponent<RectTransform>();
            entryRectTransform.anchoredPosition = new Vector2(0, -2 * i);
            entryTransform.gameObject.SetActive(true);


            SetTextOnComponent(entryTransform, "IdText", (i + 1).ToString());
            SetTextOnComponent(entryTransform, "PlayerNameText", _highScoresResponse.Results[i].PlayerName);
            SetTextOnComponent(entryTransform, "ScoreText", _highScoresResponse.Results[i].Score.ToString());
        }

        entryTemplate.gameObject.SetActive(false);

    }

    private void SetTextOnComponent(Transform parentTransform, string childName, string textValue)
    {
        Text text = parentTransform.Find(childName).GetComponent<Text>();
        if (text != null)
        {
            text.text = textValue;
        }
        else
        {
            Debug.LogError($"Text component not found on '{childName}' game object.");
        }
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
}