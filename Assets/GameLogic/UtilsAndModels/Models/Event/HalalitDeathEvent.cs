using Assets.Enums;
using System;
using UnityEngine;

public class HalalitDeathEvent : MonoBehaviour
{
    private static KeyValuePair<EventName, EventHandler<HalalitDeathEventArguments>> _halalitDeathEvent;

    public static event EventHandler<HalalitDeathEventArguments> HalalitDeath;

    void Start()
    {
        _halalitDeathEvent = new(EventName.HALALIT_DEATH, HalalitDeath);
    }

    public static void InvokeHalalitDeath(object initiator, HalalitDeathEventArguments deathEventArguments)
    {
        Event.Invoke(_halalitDeathEvent.Value, initiator, deathEventArguments);
    }
}

