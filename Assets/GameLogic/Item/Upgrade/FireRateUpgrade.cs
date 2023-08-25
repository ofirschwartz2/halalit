using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class FireRateUpgrade : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _cooldownMultiplier;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    // TODO (dev): Make this upgrade work per shot, not for all shots all together

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.COOLDOWN_MULTIPLIER, _cooldownMultiplier }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.FIRE_RATE, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}
