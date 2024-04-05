using System;
using UnityEngine;

public class Event : MonoBehaviour
{
    public static void Invoke<T>(EventHandler<T> eventHandler, object initiator, T eventArguments) 
    {
        eventHandler?.Invoke(initiator, eventArguments);
    }
}