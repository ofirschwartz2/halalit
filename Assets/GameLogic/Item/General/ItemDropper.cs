using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [SerializeField]
    private ItemRanker itemRanker;
    [SerializeField]
    private List<KeyValuePair<ItemName, GameObject>> _weapons;
    [SerializeField]
    private List<KeyValuePair<ItemName, GameObject>> _upgrades;
    [SerializeField]
    private List<KeyValuePair<ItemName, GameObject>> _utilities;

    private Dictionary<ItemName, KeyValuePair<float, GameObject>> _itemsDictionary;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DropEvent.ItemDrop += DropItem;
    }

    void Start()
    {
        InitItemDictionaries();
    }

    private void InitItemDictionaries()
    {
        _itemsDictionary = new();

        foreach (KeyValuePair<ItemName, GameObject> weapon in _weapons)
        {
            _itemsDictionary.Add(weapon.Key, new(itemRanker.GetRank(weapon.Key), weapon.Value));
        }

        foreach (KeyValuePair<ItemName, GameObject> upgrade in _upgrades)
        {
            _itemsDictionary.Add(upgrade.Key, new(itemRanker.GetRank(upgrade.Key), upgrade.Value));
        }

        foreach (KeyValuePair<ItemName, GameObject> utility in _utilities)
        {
            _itemsDictionary.Add(utility.Key, new(itemRanker.GetRank(utility.Key), utility.Value));
        }
    }
    #endregion

    #region Item Dropping
    private void DropItem(object initiator, DropEventArguments arguments)
    {
        if (arguments.DropType == DropType.ITEM_DROP)
        {
            GameObject randomValidByRankItem = GetRandomValidByRankItem(arguments);
            InstantiateItem(randomValidByRankItem, arguments.DropPosition, arguments.DropForce);
        }
    }

    private GameObject GetRandomValidByRankItem(DropEventArguments arguments)
    {
        Dictionary<ItemName, KeyValuePair<float, GameObject>> validItemsByRank = _itemsDictionary.Keys.Where(
                        itemName =>
                        _itemsDictionary[itemName].Key >= arguments.Luck.min &&
                        _itemsDictionary[itemName].Key <= arguments.Luck.max)
                        .ToDictionary(itemName => itemName, itemName => _itemsDictionary[itemName]);

        return validItemsByRank.ElementAt(Random.Range(0, validItemsByRank.Count)).Value.Value;
    }

    public void InstantiateItem(GameObject itemPrefab, Vector2 position, Vector2 dropForce)
    {
        GameObject item = Instantiate(itemPrefab, position, Quaternion.identity);

        item.transform.SetParent(transform.parent);
        item.GetComponent<Rigidbody2D>().AddForceAtPosition(dropForce, item.transform.position);
    }
    #endregion
}
