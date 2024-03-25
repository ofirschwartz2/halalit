using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
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
    private void TryDropNewItem(object initiator, DropEventArguments arguments)
    {
        var dropChance = RandomGenerator.GetRandomFloatBetweenZeroToOne(true);
        List<ItemName> droppableItemsByChance = GetDroppableItemsByChance(arguments.Dropper, dropChance);

        if (droppableItemsByChance.Count > 0)
        {
            var randomlyPickedDroppableItemByChanceIndex = RandomGenerator.GetRandomInt(0, droppableItemsByChance.Count, true);
            DropNewItem(droppableItemsByChance[randomlyPickedDroppableItemByChanceIndex], arguments.DropPosition, arguments.DropForce);
        }
    }
    #endregion
}
