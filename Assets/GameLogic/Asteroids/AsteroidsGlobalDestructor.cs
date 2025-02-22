﻿using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class AsteroidsGlobalDestructor : MonoBehaviour
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

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        DeathEvent.AsteroidDeath -= OnAsteroidDeath;
    }
    #endregion

    #region Asteroid destruction
    private void OnAsteroidDeath(object initiator, TargetDeathEventArguments arguments)
    {
        GameObject asteroidToKill = ((MonoBehaviour)initiator).gameObject;

        InvokeItemDropEvent(asteroidToKill);
        InvokeAsteroidInternalInstantiationEvent(asteroidToKill);
        TryInvokeValuableDropEvent(asteroidToKill);
        Destroy(asteroidToKill);
    }

    private void TryInvokeValuableDropEvent(GameObject asteroidToKill)
    {
        var potentialValuableDrops = asteroidToKill.GetComponent<PotentialValuableDrops>().GetValuablesToChances();
        foreach (var potentialValuableDrop in potentialValuableDrops)
        {
            float randomSeededNumber = asteroidToKill.GetComponent<AsteroidSharedBehavior>()._seedfulRandomGenerator.RangeZeroToOne();
            if (randomSeededNumber <= potentialValuableDrop.Value)
            {
                Vector2 dropForce = SeedlessRandomGenerator.GetInsideUnitCircle() * asteroidToKill.transform.localScale.x;
                ValuableDropEventArguments valuableDropEventArguments = new(asteroidToKill.GetComponent<AsteroidDropper>().GetDropper(), asteroidToKill.transform.position, dropForce, potentialValuableDrop.Key);
                ValuableDropEvent.Invoke(EventName.NEW_VALUABLE_DROP, this, valuableDropEventArguments);
                return;
            }
        }
    }

    private void InvokeItemDropEvent(GameObject asteroidToKill)
    {
        Vector2 dropForce = SeedlessRandomGenerator.GetInsideUnitCircle() * asteroidToKill.transform.localScale.x;
        ItemDropEventArguments itemDropEventArguments = new(Dropper.ASTEROID, asteroidToKill.transform.position, dropForce);
        ItemDropEvent.Invoke(EventName.NEW_ITEM_DROP, this, itemDropEventArguments);
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