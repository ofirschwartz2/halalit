using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class Score : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ValuableName, int>> _valuableValues;
   
    public Text scoreText;

    private int _score;

    private void Awake()
    {
        _score = 0;
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        ValuableEvent.PlayerValuablePickUp += IncreaseScore;
    }

    private void IncreaseScore(object initiator, ValuableEventArguments arguments)
    {
        _score += _valuableValues.Find(valuable => valuable.Key == arguments.Name).Value;
        scoreText.text = "Score: " + _score.ToString();
    }
}