using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

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
        ItemDropEvent.NewItemDrop += TryDropNewItem;
    }
    #endregion

    #region Destroy

    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        ItemDropEvent.NewItemDrop -= TryDropNewItem;
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

#if UNITY_EDITOR
internal
#else
private
#endif
    void DropNewItem(ItemName itemName, Vector2 dropPosition, Vector2 dropForce)
    {
        GameObject item = _itemsBank.GetItem(itemName);

        item.transform.SetPositionAndRotation(dropPosition, Quaternion.identity);
        item.transform.SetParent(transform);
        item.GetComponent<Rigidbody2D>().AddForce(dropForce, ForceMode2D.Impulse); 
    }
#endregion

    #region Events actions
    private void TryDropNewItem(object initiator, ItemDropEventArguments arguments)
    {
        var dropChance = RandomGenerator.RangeZeroToOne(true);
        List<ItemName> droppableItemsByChance = GetDroppableItemsByChance(arguments.Dropper, dropChance);

        if (droppableItemsByChance.Count > 0)
        {
            var randomlyPickedDroppableItemByChanceIndex = RandomGenerator.Range(0, droppableItemsByChance.Count, true);
            DropNewItem(droppableItemsByChance[randomlyPickedDroppableItemByChanceIndex], arguments.DropPosition, arguments.DropForce);
        }
    }
    #endregion
}
