using Assets.Enums;
using System;
using UnityEngine;

public class AsteroidGlobalDestructor : MonoBehaviour
{
    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DeathEvent.AsteroidDeath += OnAsteroidDeath;
    }
    #endregion

    #region Asteroid destruction
    private void OnAsteroidDeath(object initiator, DeathEventArguments arguments)
    {
        GameObject asteroidToKill = ((MonoBehaviour)initiator).gameObject;

        InvokeItemDropEvent(asteroidToKill);
        InvokeAsteroidInternalInstantiationEvent(asteroidToKill);
        Destroy(asteroidToKill);
    }

    private void InvokeItemDropEvent(GameObject asteroidToKill)
    {
        Vector2 dropForce = UnityEngine.Random.onUnitSphere * asteroidToKill.transform.localScale.x;
        DropEventArguments itemDropEventArguments = new(Dropper.ASTEROID, asteroidToKill.transform.position, dropForce);
        DropEvent.Invoke(EventName.NEW_ITEM_DROP, this, itemDropEventArguments);
    }

    private void InvokeAsteroidInternalInstantiationEvent(GameObject asteroidToKill)
    {
        AsteroidMovement asteroidToKillMovement = asteroidToKill.GetComponent<AsteroidMovement>();

        AsteroidEventArguments asteroidEventArguments = new(
            asteroidToKill.transform.position,
            asteroidToKillMovement.GetSpeed(),
            asteroidToKillMovement.GetDirection(),
            Convert.ToInt32(asteroidToKill.transform.localScale.x));

        AsteroidEvent.Invoke(EventName.ASTEROID_INTERNAL_INSTANTIATION, this, asteroidEventArguments);
    }
    #endregion
}