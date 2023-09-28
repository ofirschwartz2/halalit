using UnityEngine;
using Assets.Enums;

public class ClawRange : Upgrade
{
    [SerializeField]
    private float _clawRangeAddition;

    void Start()
    {
        _itemName = ItemName.CLAW_RANGE;
        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_RANGE_ADDITION, _clawRangeAddition }
        };
    }
}