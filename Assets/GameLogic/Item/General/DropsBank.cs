﻿using UnityEngine;
using Assets.Enums;
using System.Collections.Generic;

public class DropsBank : MonoBehaviour
{
    [SerializeField]
    private ItemsBank _itemsBank;
    [SerializeField]
    private List<KeyValuePair<Dropper, List<DropDto>>> _allDefaultDropsList;

    private Dictionary<Dropper, Dictionary<ItemName, float>> _allDroppables;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        ItemsBankEvent.NoStock += RemoveDroppable;
    }

    private void Start()
    {
        InitAllDroppables();
    }

    private void InitAllDroppables()
    {
        _allDroppables = new();

        foreach (KeyValuePair<Dropper, List<DropDto>> dropperDrops in _allDefaultDropsList)
        {
            _allDroppables[dropperDrops.Key] = new();

            foreach (DropDto drop in dropperDrops.Value)
            {
                if (_itemsBank.IsAvailable(drop.ItemName))
                {
                    _allDroppables[dropperDrops.Key].Add(drop.ItemName, drop.GetDropChance());
                }
            }
        }
    }
    #endregion

    #region Accessors
    public Dictionary<ItemName, float> GetDrops(Dropper dropper)
    {
        return _allDroppables[dropper];
    }
    #endregion

    #region Events actions
    private void RemoveDroppable(object initiator, ItemsBankEventArguments arguments)
    {
        foreach (Dropper dropper in _allDroppables.Keys)
        {
            _allDroppables[dropper].Remove(arguments.ItemName);
        }
    }
    #endregion
}
