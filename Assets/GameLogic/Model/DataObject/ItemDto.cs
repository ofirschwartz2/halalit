using Assets.Enums;
using System;
using UnityEngine;

[Serializable]
public class ItemDto
{ 
    public ItemName ItemName;
    public ItemRank ItemRank;
    public GameObject GameObject;

    public ItemDto(ItemName itemName, ItemRank itemRank, GameObject gameObject)
    {
        ItemName = itemName;
        ItemRank = itemRank;
        GameObject = gameObject;
    }
}