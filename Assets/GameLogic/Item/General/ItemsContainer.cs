using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class ItemsContainer : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<ItemRank, float>> _minimumLuckPerRank;
    [SerializeField]
    private List<ItemDto> _attacks;
    [SerializeField]
    private List<ItemDto> _upgrades;
    [SerializeField]
    private List<ItemDto> _utilities;

    private Dictionary<ItemRank, List<ItemDto>> _allItemsByRank;

    #region Init
    void Start()
    {
        InitAllItemsByRank();
    }

    private void InitAllItemsByRank()
    {
        _allItemsByRank = new()
        {
            { ItemRank.COMMON, new() },
            { ItemRank.UNCOMMON, new() },
            { ItemRank.RARE, new() },
            { ItemRank.EXCLUSIVE, new() },
            { ItemRank.EPIC, new() },
            { ItemRank.LEGENDARY, new() },
        };

        foreach (ItemDto weapon in _attacks)
        {
            _allItemsByRank[weapon.ItemRank].Add(weapon);
        }

        foreach (ItemDto upgrade in _upgrades)
        {
            _allItemsByRank[upgrade.ItemRank].Add(upgrade);
        }

        foreach (ItemDto utility in _utilities)
        {
            _allItemsByRank[utility.ItemRank].Add(utility);
        }
    }
    #endregion

    #region Accessors
    public GameObject GetRandomValidItem(RangeAttribute luckRange)
    {
        float luck = Random.Range(luckRange.min, luckRange.max);
        ItemRank itemRankByLuck = GetItemRank(luck);
        int selectedItemIndex = Random.Range(0, _allItemsByRank[itemRankByLuck].Count);

        return _allItemsByRank[itemRankByLuck][selectedItemIndex].GameObject;
    }

    private ItemRank GetItemRank(float luck)
    {
        for (int i = _minimumLuckPerRank.Count - 1; i >= 0; i--)
        {
            if (luck > _minimumLuckPerRank[i].Value)
            {
                return _minimumLuckPerRank[i].Key;
            }
        }

        Debug.Log("Invalid luck");
        throw new System.Exception("Invalid luck: " + luck);
    }
    #endregion
}
