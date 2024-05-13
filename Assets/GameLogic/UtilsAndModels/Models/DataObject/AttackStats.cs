using Assets.Enums;
using System;
using UnityEngine;

[Serializable]
public class AttackStats : IItemStats
{
    [SerializeField]
    private ItemRank _itemRank;
    public int Power;
    public float CriticalHit;
    public int Luck;
    public float Rate;
    public float Weight;

    public AttackStats(ItemRank itemRank, int power, float criticalHit, int luck, float rate, float weight)
    {
        _itemRank = itemRank;
        Power = power;
        CriticalHit = criticalHit;
        Luck = luck;
        Rate = rate;
        Weight = weight;
    }
}