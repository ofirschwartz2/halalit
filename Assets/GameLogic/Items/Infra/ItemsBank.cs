using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class ItemsBank : MonoBehaviour
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
        foreach (ItemStockDto attackItemStockDto in _attacks)
        {
            var stock = RandomGenerator.GetRandomInt(attackItemStockDto.MinimumInitialStock, attackItemStockDto.MaxStock + 1, true);

            for (int i = 0; i < stock; i++)
            {
                attackItemStockDto.Stock.Insert(0, GenerateAttackDto(attackItemStockDto.ItemName));
            }
        }

        foreach (ItemStockDto upgradeItemStockDto in _upgrades)
        {
            var stock = RandomGenerator.GetRandomInt(upgradeItemStockDto.MinimumInitialStock, upgradeItemStockDto.MaxStock + 1);

            for (int i = 0; i < stock; i++)
            {
                upgradeItemStockDto.Stock.Insert(0, new UpgradeStats());
            }
        }

        foreach (ItemStockDto utilityItemStockDto in _utilities)
        {
            var stock = RandomGenerator.GetRandomInt(utilityItemStockDto.MinimumInitialStock, utilityItemStockDto.MaxStock + 1);

            for (int i = 0; i < stock; i++)
            {
                utilityItemStockDto.Stock.Insert(0, new UtilityStats());
            }
        }
    }

    private AttackStats GenerateAttackDto(ItemName itemName)
    {
        ItemRank itemRank = _itemRankPicker.PickAnItemRank();
        AttackOptions attackPossibleStats = _itemOptions.GetAttackPossibleStats(itemName);
        AttackStatsRange attackStatsRange = attackPossibleStats.GetAttackDtoRange(itemRank);
        return attackStatsRange.GetRandom();
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
