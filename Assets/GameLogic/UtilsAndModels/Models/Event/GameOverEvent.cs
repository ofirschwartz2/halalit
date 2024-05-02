using Assets.Enums;
using System;
using UnityEngine;

public class GameOverEvent : MonoBehaviour
{
    private static KeyValuePair<EventName, EventHandler<GameOverEventArguments>> _gameOverEvent;

    public static event EventHandler<GameOverEventArguments> GameOver;

    void Start()
    {
        _gameOverEvent = new(EventName.GAME_OVER, GameOver);
    }

    public static void InvokeGameOver(object initiator, GameOverEventArguments deathEventArguments)
    {
        Event.Invoke(_gameOverEvent.Value, initiator, deathEventArguments);
    }
}

