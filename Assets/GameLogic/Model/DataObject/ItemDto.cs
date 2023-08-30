using Assets.Enums;
using System;
using UnityEngine;

[Serializable]
public class ItemDto
{ 
    public ItemName ItemName;
    public GameObject GameObject;
    public int MaxStock;
    public int MinimumInitialStock;
    public int Stock;
}