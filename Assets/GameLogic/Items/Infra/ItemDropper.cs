using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

public class ItemDropper : SeedfulRandomGeneratorUser
{
    [SerializeField]
    private ItemsBank _itemsBank;
    [SerializeField]
    private DropsBank _dropsBank;
    [SerializeField]
    private GameObject _mimicPrefab;
    [SerializeField]
    private int _mimicOneInChance = 6;

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

    // One weapon item drop in _mimicOneInChance is a Mimic, an enemy that looks like a weapon item. Takes no stock.
    // Draws only when a Mimic could drop: never for upgrades or utilities, and never from a dying Mimic.
#if UNITY_EDITOR
internal
#else
private
#endif
    bool RollForMimic(Dropper dropper, ItemName itemName)
    {
        if (_mimicPrefab == null || dropper == Dropper.MIMIC || !_itemsBank.IsAttackItem(itemName))
        {
            return false;
        }

        return _seedfulRandomGenerator.Range(0, _mimicOneInChance) == 0;
    }

    // The Mimic's seed is set at once, before its Start, from this dropper's seeded generator.
    private void DropMimic(Vector2 dropPosition, Vector2 dropForce)
    {
        GameObject mimic = Instantiate(_mimicPrefab, dropPosition, Quaternion.identity, transform);
        mimic.GetComponent<ISeedfulRandomGeneratorUser>().SetInitialSeedfulRandomGenerator(_seedfulRandomGenerator.GetNumber());
        mimic.GetComponent<Rigidbody2D>().AddForce(dropForce, ForceMode2D.Impulse);
    }
#endregion

    #region Events actions
    private void TryDropNewItem(object initiator, ItemDropEventArguments arguments)
    {
        var dropChance = _seedfulRandomGenerator.RangeZeroToOne();
        List<ItemName> droppableItemsByChance = GetDroppableItemsByChance(arguments.Dropper, dropChance);

        if (droppableItemsByChance.Count > 0)
        {
            var randomlyPickedDroppableItemByChanceIndex = _seedfulRandomGenerator.Range(0, droppableItemsByChance.Count);
            var itemName = droppableItemsByChance[randomlyPickedDroppableItemByChanceIndex];

            if (RollForMimic(arguments.Dropper, itemName))
            {
                DropMimic(arguments.DropPosition, arguments.DropForce);
            }
            else
            {
                DropNewItem(itemName, arguments.DropPosition, arguments.DropForce);
            }
        }
    }
    #endregion

#if UNITY_EDITOR
    internal GameObject GetMimicPrefab()
    {
        return _mimicPrefab;
    }

    internal int GetMimicOneInChance()
    {
        return _mimicOneInChance;
    }
#endif
}
