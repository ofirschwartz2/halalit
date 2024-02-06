using UnityEngine;
using Assets.Enums;

public class WeaponStamina : Upgrade
{
    [SerializeField]
    private float _weaponCooldownMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.WEAPON_STAMINA;
        _pickupEventProperties = new()
        {
            { EventProperty.WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION, _weaponCooldownMultiplierSubtruction }
        };
    }
}