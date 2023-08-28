using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class ClawAgility : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _clawSpeedMultiplierAddition;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_SPEED_MULTIPLIER_ADDITION, _clawSpeedMultiplierAddition }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.CLAW_AGILITY, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}


