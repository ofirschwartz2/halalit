using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private float _cooldownInterval;
    [SerializeField]
    private float _attackJoystickEdge;

    private float _cooldownTime;
    private AttackToggle _attackToggle;
    private Dictionary<ItemName, Action<Dictionary<EventProperty, float>>> _upgradeActions;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        SetUpgradeActions();
    }

    private void SetEventListeners()
    {
        ItemEvent.PlayerUpgradePickUp += UpgradeWeapon;
    }

    private void SetUpgradeActions()
    {
        _upgradeActions = new()
        {
            { ItemName.WEAPON_STAMINA, UpgradeCooldownInterval }
        };
    }

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _attackToggle = gameObject.GetComponentInParent<AttackToggle>();
        _cooldownTime = 0;
    }
    #endregion

    #region Logic
    void Update()
    {
        if (ShouldAttack() && IsCoolDownPassed())
        {
            Attack();
        }
    }

    private void Attack()
    {
        GameObject attackPrefab = _attackToggle.GetAttackPrefab();
        Instantiate(attackPrefab, transform.position, transform.rotation);
        _cooldownTime = Time.time + _cooldownInterval;
    }
    #endregion

    #region Events actions
    private void UpgradeWeapon(object initiator, ItemEventArguments arguments)
    {
        if (IsRelevantUpgradeEvent(arguments))
        {
            _upgradeActions[arguments.Name].Invoke(arguments.Properties);
        }
    }

    private void UpgradeCooldownInterval(Dictionary<EventProperty, float> properties)
    {
        _cooldownInterval -= properties[EventProperty.WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION];
        Debug.Log("Weapon stamina upgraded");
    } 
    #endregion

    #region Predicates
    private bool IsCoolDownPassed()
    {
        return Time.time >= _cooldownTime;
    }

    private bool ShouldAttack()
    {
        return Utils.GetLengthOfLine(_attackJoystick.Horizontal, _attackJoystick.Vertical) > _attackJoystickEdge;
    }

    private bool IsRelevantUpgradeEvent(ItemEventArguments arguments)
    {
        return _upgradeActions.ContainsKey(arguments.Name);
    }
    #endregion
}
