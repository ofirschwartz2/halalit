using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class ClawRange : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _clawRangeAddition;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.CLAW_RANGE_ADDITION, _clawRangeAddition }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.CLAW_RANGE, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}


