using UnityEngine;
using Assets.Enums;

public class HalalitVigor : Upgrade
{
    [SerializeField]
    private float _halalitVigorMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.HALALIT_VIGOR;
        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_VIGOR_MULTIPLIER_SUBTRUCTION, _halalitVigorMultiplierSubtruction }
        };
    }
}