using Assets.Enums;
using System;
using System.Collections.Generic;

[Serializable]
public class AttackStatsRange
{
    public Dictionary<AttackStatsType, MinMaxRange> StatsRanges;

    private ItemRank _itemRank;

    public void SetRank(ItemRank itemRank)
    {
        _itemRank = itemRank;
    }

    public AttackStats GetAttackStats(int power, float criticalHit, float luck, float rate, float weight) 
    {
        return new AttackStats(_itemRank, power, criticalHit, luck, rate, weight);
    }

    public Dictionary<AttackStatsType, MinMaxRange> GetStatsRanges() 
    {
        return StatsRanges;
    }
}