using UnityEngine;
using Assets.Utils;
using Assets.Enums;
using System.Collections.Generic;

public class WeaponStrength : Upgrade
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _weaponMovementDragMultiplierSubtruction;

    private Dictionary<EventProperty, float> _pickupEventProperties;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _pickupEventProperties = new()
        {
            { EventProperty.WEAPON_MOVEMENT_DRAG_MULTIPLIER_SUBTRUCTION, _weaponMovementDragMultiplierSubtruction }
        };
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUpgradePickedUp(this, new(ItemName.WEAPON_STRENGH, _pickupEventProperties));
            Destroy(gameObject);
        }
    }
}
