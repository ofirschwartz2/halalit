using UnityEngine;
using Assets.Utils;
using Assets.Enums;

public class SwordItem : AttackItem
{
    private void Start()
    {
        _itemName = ItemName.SWORD;
    }
}
