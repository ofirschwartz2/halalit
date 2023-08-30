using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class HalalitImmunity : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _halalitDefenceMultiplierSubtruction;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_DEFENCE_MULTIPLIER_SUBTRUCTION, _halalitDefenceMultiplierSubtruction }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.HALALIT_IMMUNITY, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}

