using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class AttackBehaviour : MonoBehaviour, IHarmer
{
    public AttackShotType ShotType;
    public AttackStats AttackStats;

    public void Copy(AttackBehaviour attackBehaviour)
    {
        ShotType = attackBehaviour.ShotType;
        AttackStats = attackBehaviour.AttackStats;
    }

    public int GetHarm()
    {
        int criticalDamage = GetCriticalDamage();
        
        if (criticalDamage > 0)
        {
            return ApplyHarmMultiplier(criticalDamage);
        }

        return ApplyHarmMultiplier(AttackStats.Power);
    }

    private int GetCriticalDamage()
    {
        bool isCriticalDamage = CombatRandomGenerator.GetSeedfulRandomGenerator().Range(0, 100) < AttackStats.Luck;

        if (isCriticalDamage)
        {
            return (int)Math.Floor(AttackStats.Power * AttackStats.CriticalHit);
        }

        return 0;
    }

    // Harm multiplier: set at runtime on the instance (Bank Shot). Not serialized; 1 keeps every other attack's harm as it was.
    private float _harmMultiplier = 1f;

    public void SetHarmMultiplier(float harmMultiplier)
    {
        _harmMultiplier = harmMultiplier;
    }

    private int ApplyHarmMultiplier(int harm)
    {
        return Mathf.CeilToInt(harm * _harmMultiplier);
    }
}