using Assets.Enums;
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

public class Score : MonoBehaviour
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
    }
    #endregion

    private void IncreaseScore(object initiator, ValuableEventArguments arguments)
    {
        _score += _valuableValues.Find(valuable => valuable.Key == arguments.Name).Value;
        scoreText.text = "Score: " + _score.ToString();
    }

    public void SetGameStats()
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
        if (_score > PlayerStats._highScore)
        {
            PlayerStats._highScore = _score;
            PlayerStats._isNewHighScore = true;
        }
    }

    public int GetHighScore()
    {
        return _score;
    }

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