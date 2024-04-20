using Assets.Enums;
using Codice.CM.Common;
using System;
using System.Collections.Generic;

[Serializable]
public class AttackOptions
{
    public ItemName attackName;
    public AttackStatsRange CommonStats;
    public AttackStatsRange UncommonStats;
    public AttackStatsRange RareStats;
    public AttackStatsRange ExclusiveStats;
    public AttackStatsRange EpicStats;
    public AttackStatsRange LegendaryStats;

    public void InitSeeds(List<int> seeds)
    {
        if (seeds.Count != Enum.GetValues(typeof(ItemRank)).Length) 
        {
            throw new Exception("Invalid number of seeds");
        }

        CommonStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.COMMON]);
        UncommonStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.UNCOMMON]);
        RareStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.RARE]);
        ExclusiveStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.EXCLUSIVE]);
        EpicStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.EPIC]);
        LegendaryStats.SetInitialSeedfulRandomGenerator(seeds[(int)ItemRank.LEGENDARY]);
    }

    public void InitRanks()
    {
        CommonStats.SetRank(ItemRank.COMMON);
        UncommonStats.SetRank(ItemRank.UNCOMMON);
        RareStats.SetRank(ItemRank.RARE);
        ExclusiveStats.SetRank(ItemRank.EXCLUSIVE);
        EpicStats.SetRank(ItemRank.EPIC);
        LegendaryStats.SetRank(ItemRank.LEGENDARY);
    }

    public AttackStatsRange GetAttackDtoRange(ItemRank itemRank)
    {
        return itemRank switch
        {
            ItemRank.COMMON => CommonStats,
            ItemRank.UNCOMMON => UncommonStats,
            ItemRank.RARE => RareStats,
            ItemRank.EXCLUSIVE => ExclusiveStats,
            ItemRank.EPIC => EpicStats,
            ItemRank.LEGENDARY => LegendaryStats,
            _ => throw new Exception("Unsuported itemRank")
        };
    }
}
