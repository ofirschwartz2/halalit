using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
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
    private AmmoToggle _ammoToggle;
    private Dictionary<UpgradeName, Action<Dictionary<EventProperty, float>>> _upgradeActions;

    private void Awake()
    {
        SetEventListeners();
        SetUpgradeActions();
    }

    private void SetEventListeners()
    {
        UpgradeEvent.PlayerUpgradePickUp += UpgradeGun;
    }

    private void SetUpgradeActions()
    {
        _upgradeActions = new()
        {
            { UpgradeName.FIRE_RATE, UpgradeCooldownInterval }
        };
    }

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _ammoToggle = gameObject.GetComponentInParent<AmmoToggle>();
        _cooldownTime = 0;
    }

    void Update()
    {
        if (ShouldFire() && IsCoolDownPassed())
        {
            Fire();
        }
    }

    #region Logic
    private void Fire()
    {
        GameObject shotPrefab = _ammoToggle.GetAmmoPrefab();
        Instantiate(shotPrefab, transform.position, transform.rotation);
        _cooldownTime = Time.time + _cooldownInterval;
    }
    #endregion

    #region Events actions
    private void UpgradeGun(object initiator, UpgradeEventArguments arguments)
    {
        if (IsRelevantUpgradeEvent(arguments))
        {
            _upgradeActions[arguments.Name].Invoke(arguments.Properties);
        }
    }

    private void UpgradeCooldownInterval(Dictionary<EventProperty, float> properties)
    {
        _cooldownInterval *= properties[EventProperty.COOLDOWN_MULTIPLIER];
        Debug.Log("Fire rate upgrade loaded");
    } 
    #endregion

    #region Predicates
    private bool IsCoolDownPassed()
    {
        return Time.time >= _cooldownTime;
    }

    private bool ShouldFire()
    {
        return Utils.GetLengthOfLine(_attackJoystick.Horizontal, _attackJoystick.Vertical) > _attackJoystickEdge;
    }

    private bool IsRelevantUpgradeEvent(UpgradeEventArguments arguments)
    {
        return _upgradeActions.ContainsKey(arguments.Name);
    }
    #endregion
}