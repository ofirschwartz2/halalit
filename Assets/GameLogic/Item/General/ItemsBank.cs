using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

class ItemsBank : MonoBehaviour
{
    [SerializeField]
    private List<ItemDto> _attacks;
    [SerializeField]
    private List<ItemDto> _upgrades;
    [SerializeField]
    private List<ItemDto> _utilities;

    private Dictionary<ItemName, ItemDto> _allItems;

    #region Init
    void Awake()
    {
        InitAllItems();
        GenerateStock();
    }

    private void InitAllItems()
    {
        _allItems = new();

        AddToAllItemsByName(_attacks);
        AddToAllItemsByName(_upgrades);
        AddToAllItemsByName(_utilities);
    }

    private void AddToAllItemsByName(List<ItemDto> items)
    {
        foreach (ItemDto item in items)
        {
            _allItems.Add(item.ItemName, item);
        }
    }

    private void GenerateStock()
    {
        foreach (ItemDto itemDto in _allItems.Values)
        {
            itemDto.Stock = Random.Range(itemDto.MinimumInitialStock, itemDto.MaxStock + 1);
        }
    }
    #endregion

    #region Accessors
    public GameObject GetItem(ItemName itemName)
    {
        ItemDto itemDto = _allItems[itemName];
        
        itemDto.Stock--;
        
        if (itemDto.Stock == 0)
        {
            ItemsBankEvent.Invoke(EventName.NO_STOCK, this, new(itemName));
        }

        return _allItems[itemName].GameObject;
    }

    public bool IsAvailable(ItemName itemName)
    {
        return _allItems[itemName].Stock > 0;
    }
    #endregion
}
