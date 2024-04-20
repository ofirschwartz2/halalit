using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsBank : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private ItemOptions _itemOptions;
    [SerializeField]
    private ItemRankPicker _itemRankPicker;
    [SerializeField]
    private List<ItemStockDto> _attacks;
    [SerializeField]
    private List<ItemStockDto> _upgrades;
    [SerializeField]
    private List<ItemStockDto> _utilities;

    private Dictionary<ItemName, ItemStockDto> _allItems;

    #region Init
    void Start()
    {
        GenerateStock();
        InitAllItems();
    }

    private void GenerateStock()
    {
        // TODO: bad smell, refactor
        foreach (ItemStockDto attackItemStockDto in _attacks)
        {
            var stock = _seedfulRandomGenerator.Range(attackItemStockDto.MinimumInitialStock, attackItemStockDto.MaxStock + 1);

            for (int i = 0; i < stock; i++)
            {
                attackItemStockDto.Stock.Insert(0, GenerateAttackStats(attackItemStockDto.ItemName));
            }
        }

        foreach (ItemStockDto upgradeItemStockDto in _upgrades)
        {
            var stock = _seedfulRandomGenerator.Range(upgradeItemStockDto.MinimumInitialStock, upgradeItemStockDto.MaxStock + 1);

            for (int i = 0; i < stock; i++)
            {
                upgradeItemStockDto.Stock.Insert(0, new UpgradeStats());
            }
        }

        foreach (ItemStockDto utilityItemStockDto in _utilities)
        {
            var stock = _seedfulRandomGenerator.Range(utilityItemStockDto.MinimumInitialStock, utilityItemStockDto.MaxStock + 1);

            for (int i = 0; i < stock; i++)
            {
                utilityItemStockDto.Stock.Insert(0, new UtilityStats());
            }
        }
    }

    private AttackStats GenerateAttackStats(ItemName itemName)
    {
        ItemRank itemRank = _itemRankPicker.PickAnItemRank();
        AttackOptions attackPossibleStats = _itemOptions.GetAttackPossibleStats(itemName);
        AttackStatsRange attackStatsRange = attackPossibleStats.GetAttackDtoRange(itemRank);
        var statsRanges = attackStatsRange.StatsRanges;

        int power = _seedfulRandomGenerator.Range((int)statsRanges[AttackStatsType.POWER].min, (int)statsRanges[AttackStatsType.POWER].max + 1);
        float criticalHit = _seedfulRandomGenerator.Range(statsRanges[AttackStatsType.CRITICAL_HIT].min, statsRanges[AttackStatsType.CRITICAL_HIT].max);
        float luck = _seedfulRandomGenerator.Range(statsRanges[AttackStatsType.LUCK].min, statsRanges[AttackStatsType.LUCK].max);
        float rate = (float)Math.Round(_seedfulRandomGenerator.Range(statsRanges[AttackStatsType.RATE].min, statsRanges[AttackStatsType.RATE].max), 2);
        float weight = _seedfulRandomGenerator.Range(statsRanges[AttackStatsType.WEIGHT].min, statsRanges[AttackStatsType.WEIGHT].max);

        return attackStatsRange.GetAttackStats(power, criticalHit, luck, rate, weight);
    }

    private void InitAllItems()
    {
        _allItems = new();

        AddToAllItemsByName(_attacks);
        AddToAllItemsByName(_upgrades);
        AddToAllItemsByName(_utilities);
    }

    private void AddToAllItemsByName(List<ItemStockDto> items)
    {
        foreach (ItemStockDto item in items)
        {
            _allItems.Add(item.ItemName, item);
        }
    }
    #endregion

    #region Accessors
    public GameObject GetItem(ItemName itemName)
    {
        ItemStockDto itemStockDto = _allItems[itemName];

        IItemStats itemDto = itemStockDto.Stock[0];
        itemStockDto.Stock.RemoveAt(0);
        
        if (itemStockDto.Stock.Count == 0)
        {
            ItemsBankEvent.Invoke(EventName.NO_STOCK, this, new(itemName));
        }

        GameObject item = Instantiate(itemStockDto.GameObject);
        item.GetComponent<ItemStats>().itemStats = itemDto;

        return item;
    }

    public bool IsAvailable(ItemName itemName)
    {
        return _allItems[itemName].Stock.Count > 0;
    }
    #endregion
}
