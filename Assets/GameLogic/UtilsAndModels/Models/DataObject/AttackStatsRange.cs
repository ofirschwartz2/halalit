using Assets.Enums;
using Assets.Utils;
using System;

[Serializable]
public class AttackStatsRange : SeedfulRandomGeneratorUser
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

    public AttackStats GetRandom() 
    {
        int power = _seedfulRandomGenerator.Range((int)Power.min, (int)Power.max + 1);
        float criticalHit = _seedfulRandomGenerator.Range(CriticalHit.min, CriticalHit.max);
        float luck = _seedfulRandomGenerator.Range(Luck.min, Luck.max);
        float rate = (float)Math.Round(_seedfulRandomGenerator.Range(Rate.min, Rate.max), 2);
        float weight = _seedfulRandomGenerator.Range(Weight.min, Weight.max);

        return new AttackStats(_itemRank, power, criticalHit, luck, rate, weight);
    }
}