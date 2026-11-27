using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Results card (#249): when the ship dies, the run freezes on a card with the score, the best and, after a daily, today's place.
// The scores are saved when the card opens, so closing the app on the card keeps them. Again is for normal runs only: the daily keeps its one try
public class ResultsCard : MonoBehaviour
{
    private const string SCORE_TEXT = "Score: ";
    private const string BEST_TEXT = "Best: ";
    private const string NEW_BEST_TEXT = "New best: ";
    private const string SAVING_TEXT = "Saving...";
    private const string NOT_ON_TODAYS_LIST_TEXT = "Not on today's list";
    private const string PLACE_COULD_NOT_LOAD_TEXT = "Place could not load";

    [SerializeField]
    private GameObject _panel;
    [SerializeField]
    private Text _scoreText;
    [SerializeField]
    private Text _bestText;
    [SerializeField]
    private Text _placeText;
    [SerializeField]
    private GameObject _dailyLabel;
    [SerializeField]
    private GameObject _againButton;

    private Score _score;
    private Func<Task> _saveRunScores;
    private Func<Task<string>> _loadTodayPlaceText;
    private bool _isOpen;
    private bool _isDaily;
    private float _timeScaleBeforeOpen;

    #region Init
    private void Awake()
    {
        // An unwired card switches itself off, so a death loads the menu as before and the Playground still loads in tests.
        // PlaygroundTests.ResultsCardTests names the missing field
        if (!IsWired())
        {
            Debug.LogWarning("ResultsCard: Panel, Score Text, Best Text, Place Text, Daily Label or Again Button is not wired in the Playground scene, so a death loads the menu.");
            enabled = false;
            return;
        }

        _panel.SetActive(false);
        _saveRunScores = PlayerStats.SaveRunScoresAsync;
        _loadTodayPlaceText = LoadTodayPlaceTextAsync;
    }

    private void Start()
    {
        _score = GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription()).GetComponent<Score>();
    }
    #endregion

    #region Destroy
    // Covers any other way out, such as a test's tear-down. The time scale is put back only while the card still holds it at 0,
    // so a time scale set since then is kept
    private void OnDestroy()
    {
        if (_isOpen && Time.timeScale == 0f)
        {
            Time.timeScale = _timeScaleBeforeOpen;
        }
    }
    #endregion

    // Called by GameOver. A second call is ignored, or the card would remember 0 as the time scale to put back
    public void Open()
    {
        if (_isOpen)
        {
            return;
        }

        _isOpen = true;
        _timeScaleBeforeOpen = Time.timeScale;
        Time.timeScale = 0f;
        StartCoroutine(ShowResultsNextFrame());
    }

    // Score hands the run's score to PlayerStats in its own game over handler, and nothing pins which handler runs first.
    // So the card reads PlayerStats and saves one frame later. A null wait still runs at time scale 0
    private IEnumerator ShowResultsNextFrame()
    {
        yield return null;

        ShowResults();
        SaveAndShowPlace();
    }

    private void ShowResults()
    {
        int score = _score.GetScore();
        int best = PlayerStats._highScore ?? score;
        _isDaily = PlayerStats._isDailyRun;

        _scoreText.text = SCORE_TEXT + score;
        _bestText.text = (PlayerStats._isNewHighScore ? NEW_BEST_TEXT : BEST_TEXT) + best;
        _placeText.text = _isDaily ? SAVING_TEXT : string.Empty;
        _dailyLabel.SetActive(_isDaily);
        _againButton.SetActive(!_isDaily);
        _panel.SetActive(true);
    }

    // _isDaily is kept from ShowResults, because the save switches PlayerStats._isDailyRun off
    private async void SaveAndShowPlace()
    {
        try
        {
            await _saveRunScores();

            if (this == null)
            {
                return;
            }

            if (!_isDaily)
            {
                return;
            }

            string placeText = await _loadTodayPlaceText();

            if (this == null)
            {
                return;
            }

            _placeText.text = placeText;
        }
        catch (Exception exception)
        {
            // A warning, not an error: being offline is expected
            Debug.LogWarning($"ResultsCard: could not save the run or load your place. {exception.Message}");

            if (this == null)
            {
                return;
            }

            if (_isDaily)
            {
                _placeText.text = PLACE_COULD_NOT_LOAD_TEXT;
            }
        }
    }

    private static async Task<string> LoadTodayPlaceTextAsync()
    {
        var playerEntry = await PlayerPlace.GetPlayerEntryAsync(Constants.DAILY_LEADERBOARD_ID);

        if (playerEntry == null)
        {
            return NOT_ON_TODAYS_LIST_TEXT;
        }

        int playerCount = await PlayerPlace.GetPlayerCountAsync(Constants.DAILY_LEADERBOARD_ID);
        return PlayerPlace.GetPlaceText(playerEntry.Rank, playerCount, true);
    }

    #region Buttons
    public void PlayAgain()
    {
        LeaveTo(SceneName.PLAYGROUND);
    }

    public void OpenLeaderboard()
    {
        LeaveTo(SceneName.LEADERBOARD);
    }

    public void OpenMenu()
    {
        LeaveTo(SceneName.MAIN_MENU);
    }
    #endregion

    // Every button leaves through here. The time scale goes back first, so the next scene does not start frozen
    private void LeaveTo(SceneName sceneName)
    {
        Time.timeScale = _timeScaleBeforeOpen;
        SceneManager.LoadScene(sceneName.GetDescription());
    }

    #region Predicates
    private bool IsWired()
    {
        return _panel != null && _scoreText != null && _bestText != null && _placeText != null && _dailyLabel != null && _againButton != null;
    }
    #endregion

#if UNITY_EDITOR
    internal void SetServices(Func<Task> saveRunScores, Func<Task<string>> loadTodayPlaceText)
    {
        _saveRunScores = saveRunScores;
        _loadTodayPlaceText = loadTodayPlaceText;
    }

    internal bool IsOpen()
    {
        return _isOpen;
    }

    internal string GetScoreText()
    {
        return _scoreText.text;
    }

    internal string GetBestText()
    {
        return _bestText.text;
    }

    internal string GetPlaceText()
    {
        return _placeText.text;
    }

    internal bool IsAgainShown()
    {
        return _againButton.activeSelf;
    }

    internal bool IsDailyLabelShown()
    {
        return _dailyLabel.activeSelf;
    }
#endif
}
