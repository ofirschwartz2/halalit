using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeathEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<DeathEventArguments>> _deathEvents;

    public static event EventHandler<DeathEventArguments> HalalitDeath;
    public static event EventHandler<DeathEventArguments> EnemyDeath;
    public static event EventHandler<DeathEventArguments> AsteroidDeath;

    void Start()
    {
        _deathEvents = new();
        _deathEvents.Add(EventName.HALALIT_DEATH, HalalitDeath);
        _deathEvents.Add(EventName.ENEMY_DEATH, EnemyDeath);
        _deathEvents.Add(EventName.ASTEROID_DEATH, AsteroidDeath);
    }

    public static void Invoke(EventName eventName, object initiator, DeathEventArguments deathEventArguments)
    {
        Event.Invoke(_deathEvents[eventName], initiator, deathEventArguments);
    }
}

