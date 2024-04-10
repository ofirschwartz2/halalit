using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<TargetDeathEventArguments>> _targetDeathEvents;
    private static KeyValuePair<EventName, EventHandler<HalalitDeathEventArguments>> _halalitDeathEvent;

    public static event EventHandler<HalalitDeathEventArguments> HalalitDeath;
    public static event EventHandler<TargetDeathEventArguments> EnemyDeath;
    public static event EventHandler<TargetDeathEventArguments> AsteroidDeath;

    void Start()
    {
        _targetDeathEvents = new();
        _halalitDeathEvent = new(EventName.HALALIT_DEATH, HalalitDeath);
        _targetDeathEvents.Add(EventName.ENEMY_DEATH, EnemyDeath);
        _targetDeathEvents.Add(EventName.ASTEROID_DEATH, AsteroidDeath);
    }

    public static void InvokeTargetDeath(EventName eventName, object initiator, TargetDeathEventArguments deathEventArguments)
    {
        Event.Invoke(_targetDeathEvents[eventName], initiator, deathEventArguments);
    }

    public static void InvokeHalalitDeath(object initiator, HalalitDeathEventArguments deathEventArguments)
    {
        Event.Invoke(_halalitDeathEvent.Value, initiator, deathEventArguments);
    }
}

