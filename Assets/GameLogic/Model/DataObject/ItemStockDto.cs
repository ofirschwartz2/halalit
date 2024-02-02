using Assets.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStockDto
{ 
    public ItemName ItemName;
    public GameObject GameObject;
    public int MaxStock;
    public int MinimumInitialStock;
    [SerializeReference]
    public List<IItemStats> Stock;
}