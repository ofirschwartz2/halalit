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

    public async void Awake()
    {
        await PlayerInitialization();

        await TryGetScores();

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

    public async Task TryGetScores()
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
        var highscoreTableContainer = FindTransformChild(transform, "HighscoreTable");

        var entryContainer = FindTransformChild(highscoreTableContainer, "LeaderboardEntryContainer");

        var entryTemplate = FindTransformChild(entryContainer, "LeaderboardEntryTemplate");


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

    // TODO: move to utils
    private Transform FindTransformChild(Transform fatherTransaform, string childName)
    {
        var transformChild = fatherTransaform.Find(childName);
        if (transformChild == null)
        {
            throw new System.Exception($"{childName} not found.");
        }

        return transformChild;
    }
    // TODO: move to utils

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