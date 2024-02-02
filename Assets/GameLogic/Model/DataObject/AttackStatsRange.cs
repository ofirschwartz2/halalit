using Assets.Enums;
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
        int power = Random.Range((int)Power.min, (int)Power.max + 1);
        float criticalHit = Random.Range(CriticalHit.min, CriticalHit.max);
        float luck = Random.Range(Luck.min, Luck.max);
        float rate = Random.Range(Rate.min, Rate.max);
        float weight = Random.Range(Weight.min, Weight.max);

        return new AttackStats(_itemRank, power, criticalHit, luck, rate, weight);
    }
}