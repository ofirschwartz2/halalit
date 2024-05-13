using Assets.Enums;
using Assets.Utils;
using System;

[Serializable]
public class AttackStatsRange
{
    public MinMaxRange Power;
    public MinMaxRange CriticalHit;
    public MinMaxRange Luck;
    public MinMaxRange Rate;
    public MinMaxRange Weight;

    private ItemRank _itemRank;

    public void SetRank(ItemRank itemRank)
    {
        _itemRank = itemRank;
    }

    public AttackStats GetRandom(SeedfulRandomGenerator seedfulRandomGenerator) 
    {
        int power = seedfulRandomGenerator.Range((int)Power.min, (int)Power.max + 1);
        float criticalHit = seedfulRandomGenerator.Range(CriticalHit.min, CriticalHit.max);
        int luck = seedfulRandomGenerator.Range((int)Luck.min, (int)Luck.max);
        float rate = (float)Math.Round(seedfulRandomGenerator.Range(Rate.min, Rate.max), 2);
        float weight = seedfulRandomGenerator.Range(Weight.min, Weight.max);

        return new AttackStats(_itemRank, power, criticalHit, luck, rate, weight);
    }
}