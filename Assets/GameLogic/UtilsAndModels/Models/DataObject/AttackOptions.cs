using Assets.Enums;
using System;

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
