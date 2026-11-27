using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class AttackBehaviour : MonoBehaviour, IHarmer
{
    public AttackShotType ShotType;
    public AttackStats AttackStats;

    // Mirror Shield (#200): melee attacks (Sword, Spikes) hurt it from the front and are never reflected.
    public bool IsMelee;

    // Tripwire (#213): the shortest time between two shots, whatever the fire rate upgrades. 0 = no floor.
    public float MinCooldown;

    public void Copy(AttackBehaviour attackBehaviour)
    {
        ShotType = attackBehaviour.ShotType;
        AttackStats = attackBehaviour.AttackStats;

        IsMelee = attackBehaviour.IsMelee;
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

    // Armor breaking: set at runtime on the instance (a full Charge Lance, #236). Not serialized; false keeps every other attack blocked by front plates as it was.
    private bool _isArmorBreaking;

    public void SetArmorBreaking(bool isArmorBreaking)
    {
        _isArmorBreaking = isArmorBreaking;
    }

    public bool IsArmorBreaking()
    {
        return _isArmorBreaking;
    }
}