using Assets.Enums;
using Assets.Utils;
using System;
using Random = UnityEngine.Random;

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

    public AttackStats GetRandom() 
    {
        int power = RandomGenerator.Range((int)Power.min, (int)Power.max + 1, true);
        float criticalHit = RandomGenerator.Range(CriticalHit.min, CriticalHit.max, true);
        float luck = RandomGenerator.Range(Luck.min, Luck.max, true);
        float rate = RandomGenerator.Range(Rate.min, Rate.max, true);
        float weight = RandomGenerator.Range(Weight.min, Weight.max, true);

        return new AttackStats(_itemRank, power, criticalHit, luck, rate, weight);
    }
}