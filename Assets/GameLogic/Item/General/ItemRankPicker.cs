using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class ItemRankPicker : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ItemRank, float>> _rankChances;

    public ItemRank PickAnItemRank()
    {
        float rankValue = Random.Range(0F, 1F); 
        
        for (int i = _rankChances.Count - 1; i >= 0; i--)
        {
            if (i == 0 || rankValue > _rankChances[i].Value)
            {
                return _rankChances[i].Key;
            }
        }

        throw new Exception("Rank chances order doesn't make sense. it must be ordered in an ascending order");
    }
}
