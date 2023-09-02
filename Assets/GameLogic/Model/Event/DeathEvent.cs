using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

class DeathEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<DeathEventArguments>> _deathEvents;

    public static event EventHandler<DeathEventArguments> HalalitDeath;

    void Start()
    {
        _deathEvents = new();
        _deathEvents.Add(EventName.HALALIT_DEATH, HalalitDeath);
    }

    public static void Invoke(EventName eventName, object initiator, DeathEventArguments deathEventArguments)
    {
        Event.Invoke(_deathEvents[eventName], initiator, deathEventArguments);
    }
}

