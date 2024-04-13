using Assets.Enums;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        _score = 0;
        SetEventListeners();
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

    public int GetScore()
    {
        return _score;
    }

#if UNITY_EDITOR
    internal List<KeyValuePair<ValuableName, int>> GetValuableValues()
    {
        return _valuableValues;
    }
#endif

}