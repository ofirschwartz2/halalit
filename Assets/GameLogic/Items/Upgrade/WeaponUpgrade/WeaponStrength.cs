using UnityEngine;
using Assets.Enums;

public class WeaponStrength : Upgrade
{
    [SerializeField]
    private float _weaponMovementDragMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.WEAPON_STRENGH;
        _pickupEventProperties = new()
        {
            { EventProperty.WEAPON_MOVEMENT_DRAG_MULTIPLIER_SUBTRUCTION, _weaponMovementDragMultiplierSubtruction }
        };
    }
}