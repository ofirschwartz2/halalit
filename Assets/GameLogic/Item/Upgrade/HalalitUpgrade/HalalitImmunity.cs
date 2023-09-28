using UnityEngine;
using Assets.Enums;

public class HalalitImmunity : Upgrade
{
    [SerializeField]
    private float _halalitDefenceMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.HALALIT_IMMUNITY;
        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_DEFENCE_MULTIPLIER_SUBTRUCTION, _halalitDefenceMultiplierSubtruction }
        };
    }
}