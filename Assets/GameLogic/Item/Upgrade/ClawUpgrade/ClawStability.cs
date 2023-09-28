using UnityEngine;
using Assets.Enums;

public class ClawStability : Upgrade
{
    [SerializeField]
    private float _clawStumbleMultiplierSubtruction;

    void Start()
    {
        _itemName = ItemName.CLAW_STABILITY;
        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_STUMBLE_MULTIPLIER_SUBTRUCTION, _clawStumbleMultiplierSubtruction }
        };
    }
}