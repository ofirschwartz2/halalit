using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

class AsteroidEvent : MonoBehaviour
{
    private static Dictionary<EventName, EventHandler<AsteroidEventArguments>> _asteroidEvents;

    public static event EventHandler<AsteroidEventArguments> AsteroidInternalInstantiation;

    void Start()
    {
        _asteroidEvents = new();
        _asteroidEvents.Add(EventName.ASTEROID_INTERNAL_INSTANTIATION, AsteroidInternalInstantiation);
    }

    public static void Invoke(EventName eventName, object initiator, AsteroidEventArguments asteroidEventArguments)
    {
        Event.Invoke(_asteroidEvents[eventName], initiator, asteroidEventArguments);
    }
}
