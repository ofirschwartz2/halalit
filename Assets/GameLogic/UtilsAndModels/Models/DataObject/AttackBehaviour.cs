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
            return criticalDamage;
        }

        return AttackStats.Power;
    }

    private int GetCriticalDamage()
    {
        bool isCriticalDamage = SeedlessRandomGenerator.Range(0, 100) < AttackStats.Luck;

        if (isCriticalDamage)
        {
            return (int)Math.Floor(AttackStats.Power * AttackStats.CriticalHit);
        }

        return 0;
    }
}