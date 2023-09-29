using UnityEngine;
using Assets.Enums;

public class ClawAgility : Upgrade
{
    [SerializeField]
    private float _clawSpeedMultiplierAddition;

    void Start()
    {
        _itemName = ItemName.CLAW_AGILITY;
        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_SPEED_MULTIPLIER_ADDITION, _clawSpeedMultiplierAddition }
        };
    }
}


