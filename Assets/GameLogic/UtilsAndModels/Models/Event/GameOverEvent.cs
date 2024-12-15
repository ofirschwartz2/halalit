using Assets.Enums;
using System;
using UnityEngine;

public class GameOverEvent : MonoBehaviour
{
    private static KeyValuePair<EventName, EventHandler<GameOverEventArguments>> _GameOverEvent;

    public static event EventHandler<GameOverEventArguments> GameOver;

    void Start()
    {
        _GameOverEvent = new(EventName.HALALIT_DEATH, GameOver);
    }

    public static void InvokeGameOver(object initiator, GameOverEventArguments deathEventArguments)
    {
        Event.Invoke(_GameOverEvent.Value, initiator, deathEventArguments);
    }
}

