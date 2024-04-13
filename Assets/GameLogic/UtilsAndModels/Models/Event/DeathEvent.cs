using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<TargetDeathEventArguments>> _targetDeathEvents;

    public static event EventHandler<TargetDeathEventArguments> EnemyDeath;
    public static event EventHandler<TargetDeathEventArguments> AsteroidDeath;

    void Start()
    {
        _targetDeathEvents = new();
        _targetDeathEvents.Add(EventName.ENEMY_DEATH, EnemyDeath);
        _targetDeathEvents.Add(EventName.ASTEROID_DEATH, AsteroidDeath);
    }

    public static void InvokeTargetDeath(EventName eventName, object initiator, TargetDeathEventArguments deathEventArguments)
    {
        Event.Invoke(_targetDeathEvents[eventName], initiator, deathEventArguments);
    }
}

