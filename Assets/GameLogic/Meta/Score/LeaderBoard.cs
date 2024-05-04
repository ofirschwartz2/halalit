using Assets.Enums;
using Assets.Utils;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LeaderBoard : MonoBehaviour
{
    const string HIGHSCORE_LEADERBOARD_ID = "1";
    private LeaderboardScoresPage _highScoresResponse;

    public async void Start() 
    {
        if (Authentication.InitializationTask == null)
        {
            await Authentication.Initialize();
        }
        else
        {
            await Authentication.InitializationTask;
        }

        await GetScores();
    }

    public async Task GetScores()
    {
        _highScoresResponse = await LeaderboardsService.Instance.GetScoresAsync(HIGHSCORE_LEADERBOARD_ID);

        var a = 0;
        var b = 0;
        Debug.Log("Leaderboard Scores:");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene(SceneName.MAIN_MENU.GetDescription());
    }
}