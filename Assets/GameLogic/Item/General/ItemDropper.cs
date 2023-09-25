using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemDropper : MonoBehaviour
{
    [SerializeField]
    private ItemsBank _itemsBank;
    [SerializeField]
    private DropsBank _dropsBank;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        DropEvent.NewItemDrop += TryDropNewItem;
    }
    #endregion

    #region Item Dropping
    private List<ItemName> GetDroppableItemsByChance(Dropper dropper, float dropChance)
    {
        Dictionary<ItemName, float> drops = _dropsBank.GetDrops(dropper);

        return drops
            .Where(itemNameChancePair => dropChance <= itemNameChancePair.Value)
            .Select(itemNameChancePair => itemNameChancePair.Key)
            .ToList();
    }

    private void DropNewItem(ItemName itemName, Vector2 dropPosition, Vector2 dropForce)
    {
        GameObject itemPrefab = _itemsBank.GetItem(itemName);
        GameObject item = Instantiate(itemPrefab, dropPosition, Quaternion.identity);

        item.transform.SetParent(transform);
        item.GetComponent<Rigidbody2D>().AddForce(dropForce, ForceMode2D.Impulse); 
    }
    #endregion

    #region Events actions
    private void TryDropNewItem(object initiator, DropEventArguments arguments)
    {
        float dropChance = Random.Range(0f, 1f);
        List<ItemName> droppableItemsByChance = GetDroppableItemsByChance(arguments.Dropper, dropChance);

        if (droppableItemsByChance.Count > 0)
        {
            int randomlyPickedDroppableItemByChanceIndex = Random.Range(0, droppableItemsByChance.Count);
            DropNewItem(droppableItemsByChance[randomlyPickedDroppableItemByChanceIndex], arguments.DropPosition, arguments.DropForce);
        }
    }
    #endregion
}
