using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class HalalitVigor : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _halalitVigorMultiplierSubtruction;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_VIGOR_MULTIPLIER_SUBTRUCTION, _halalitVigorMultiplierSubtruction }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.HALALIT_VIGOR, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}

