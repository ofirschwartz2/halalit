using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

class ItemRankPicker : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ItemRank, float>> _rankChances;

    public ItemRank PickAnItemRank()
    {
        float rankValue = RandomGenerator.RangeZeroToOne(true); 
        
        for (int i = _rankChances.Count - 1; i >= 0; i--)
        {
            if (rankValue > _rankChances[i].Value)
            {
                return _rankChances[i].Key;
            }
        }

        throw new Exception("Rank chances order doesn't make sense. it must be ordered in an ascending order");
    }
}
