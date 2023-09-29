using UnityEngine;
using Assets.Enums;

public class WeaponDexterity : Upgrade
{
    [SerializeField]
    private float _weaponShakeMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.WEAPON_DEXTERITY;
        _pickupEventProperties = new()
        {
            { EventProperty.WEAPON_SHAKE_MULTIPLIER_SUBTRUCTION, _weaponShakeMultiplierSubtruction }
        };
    }
}