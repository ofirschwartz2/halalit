using Assets.Enums;
using Assets.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class Score : MonoBehaviour, IScoreWallet
{
    [SerializeField]
    private List<KeyValuePair<ValuableName, int>> _valuableValues;

    [SerializeField]
    private Text scoreText;

    private int _score;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        _score = 0;
    }

    private void SetEventListeners()
    {
        ValuableEvent.PlayerValuablePickUp += IncreaseScore;
        GameOverEvent.GameOver += SetGameStats;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        ValuableEvent.PlayerValuablePickUp -= IncreaseScore;
        GameOverEvent.GameOver -= SetGameStats;
    }
    #endregion

    private void IncreaseScore(object initiator, ValuableEventArguments arguments)
    {
        _score += GetValuableValue(arguments.Name);
        ShowScore();
    }

    public bool TrySpend(ValuableName valuableName)
    {
        var cost = GetValuableValue(valuableName);
        if (!CanSpend(cost))
        {
            return false;
        }

        _score -= cost;
        ShowScore();
        return true;
    }

    private int GetValuableValue(ValuableName valuableName)
    {
        return _valuableValues.Find(valuable => valuable.Key == valuableName).Value;
    }

    private void ShowScore()
    {
        scoreText.text = "Score: " + _score.ToString();
    }

    private void SetGameStats(object initiator, GameOverEventArguments arguments)
    {
        TrySetHighScore();

        TrySetDailyScore();
    }

    private void TrySetDailyScore()
    {
        if (PlayerStats._isDailyRun) 
        {
            PlayerStats._dailyScore = _score;
        }
    }

    private void TrySetHighScore()
    {
        if (_score > (PlayerStats._highScore ?? 0))
        {
            PlayerStats._highScore = _score;
            PlayerStats._isNewHighScore = true;
        }
    }

    public int GetHighScore()
    {
        return _score;
    }

    #region Predicates
    private bool CanSpend(int amount)
    {
        return _score >= amount;
    }
    #endregion

#if UNITY_EDITOR

    public int GetScore()
    {
        return _score;
    }

    internal List<KeyValuePair<ValuableName, int>> GetValuableValues()
    {
        return _valuableValues;
    }

#endif

}