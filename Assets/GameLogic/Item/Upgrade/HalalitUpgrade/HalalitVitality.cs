using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class HalalitVitality : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _halalitHpAddition;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.HALALIT_HP_ADDITION, _halalitHpAddition }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.HALALIT_VITALITY, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}

