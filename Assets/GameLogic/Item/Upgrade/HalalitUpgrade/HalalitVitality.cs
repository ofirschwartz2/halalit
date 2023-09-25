using UnityEngine;
using Assets.Enums;

public class HalalitVitality : Upgrade
{
    [SerializeField]
    private float _halalitHpAddition;

    void Start()
    {
        _itemName = ItemName.HALALIT_VITALITY;
        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_HP_ADDITION, _halalitHpAddition }
        };
    }
}