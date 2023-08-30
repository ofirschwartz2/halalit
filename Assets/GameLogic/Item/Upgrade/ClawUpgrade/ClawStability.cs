using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class ClawStability : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _clawStumbleMultiplierSubtruction;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_STUMBLE_MULTIPLIER_SUBTRUCTION, _clawStumbleMultiplierSubtruction }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.CLAW_STABILITY, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}


