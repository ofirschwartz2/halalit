using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class WeaponDexterity : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _weaponShakeMultiplierSubtruction;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.WEAPON_SHAKE_MULTIPLIER_SUBTRUCTION, _weaponShakeMultiplierSubtruction }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.WEAPON_DEXTERITY, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}
