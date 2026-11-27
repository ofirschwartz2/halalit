using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

class WeaponAttack : MonoBehaviour
{
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private GameObject _projectiles;
    [SerializeField]
    private float _cooldownMultiplier;
    [SerializeField]
    private float _attackJoystickEdge;
    [SerializeField]
    private KeyValuePair<AttackName, AttackStats> _currentAttack;    // just for view in the inspector

    private AttackToggle _attackToggle;
    private Dictionary<ItemName, Action<Dictionary<EventProperty, float>>> _upgradeActions;
    private float _cooldownTime;
    private ConsecutiveAttack _consecutiveAttack;
    private bool _shootingConsecutivaly, _isTesting;
    private HalalitMovement _halalitMovement;
    private SpriteRenderer _spriteRenderer;
    private Color _baseColor;
    private GameObject _chargedAttackPrefab;
    private ChargedAttack _chargedAttack;
    private bool _isCharging;
    private float _chargeStartTime;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        SetUpgradeActions();
        _halalitMovement = _halalit.GetComponent<HalalitMovement>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _baseColor = _spriteRenderer.color;
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
        _attackToggle = gameObject.GetComponentInParent<AttackToggle>();
        _cooldownTime = 0;
        _isTesting = false;
    }
    #endregion

    #region Destroy
    private void OnDestroy()
    {
        DestroyEventListeners();
    }

    public void DestroyEventListeners()
    {
        ItemEvent.PlayerUpgradePickUp -= UpgradeWeapon;
    }
    #endregion

    #region Logic
    void FixedUpdate()
    {
        
        #if UNITY_EDITOR
        if (!_isTesting) // In testing mode, we don't want to DOUBLE update
        #endif
            HumbleFixedUpdate(new Vector2(_attackJoystick.Vertical, _attackJoystick.Horizontal));
            
    }

#if UNITY_EDITOR
    internal
#else
    private
#endif
    bool HumbleFixedUpdate(Vector2 attackJoystickTouch)
    {
        // TODO (refactor): this should be via event raise, not in update
        KeyValuePair<AttackName, GameObject> attackPrefab = _attackToggle.GetCurrentAttack();
        _currentAttack.Key = attackPrefab.Key;
        _currentAttack.Value = attackPrefab.Value.GetComponent<AttackBehaviour>().AttackStats;
        AttackShotType shotType = attackPrefab.Value.GetComponent<AttackBehaviour>().ShotType;
        // TODO (refactor): this should be via event raise, not in update

        TrySwitchChargedAttack(attackPrefab.Value);

        return TryAttack(attackJoystickTouch, attackPrefab.Value, shotType);
    }

    // Charge Lance (#236): on an attack switch, a charge in progress ends, and the new prefab's ChargedAttack is cached (null for other attacks).
    private void TrySwitchChargedAttack(GameObject attackPrefab)
    {
        if (attackPrefab != _chargedAttackPrefab)
        {
            if (_isCharging)
            {
                StopCharging();
            }

            _chargedAttackPrefab = attackPrefab;
            _chargedAttack = attackPrefab.GetComponent<ChargedAttack>();
        }
    }

    private bool TryAttack(Vector2 attackJoystickTouch, GameObject attackPrefab, AttackShotType shotType)
    {
        if (_chargedAttack != null)
        {
            return TryChargedAttack(attackJoystickTouch, attackPrefab);
        }

        if (shotType == AttackShotType.DESCRETE)
        {
            return TryDescreteAttack(attackJoystickTouch, attackPrefab);
        }
        else
        {
            return TryConsecutiveAttack(attackJoystickTouch, attackPrefab);
        }
    }

    private bool TryDescreteAttack(Vector2 attackJoystickTouch, GameObject attackPrefab)
    {
        if (ShouldAttack(attackJoystickTouch) && IsCoolDownPassed())
        {
            if (_consecutiveAttack != null)
            {
                RemoveConsecutiveAttack();
            }

            InstantiateDescreteAttack(attackPrefab);
            return true;
        }

        return false;
    }

    private GameObject InstantiateDescreteAttack(GameObject attackPrefab)
    {
        GameObject attack = Instantiate(attackPrefab, transform.position, transform.rotation, _projectiles.transform);
        _cooldownTime = Time.time + Mathf.Max(_currentAttack.Value.Rate * _cooldownMultiplier, attackPrefab.GetComponent<AttackBehaviour>().MinCooldown);
        return attack;
    }

    // Charge Lance (#236): holding the stick past the edge charges, and letting go fires, unless the hold was too short.
    // The cooldown runs from the release, and a charge starts only after it.
    private bool TryChargedAttack(Vector2 attackJoystickTouch, GameObject attackPrefab)
    {
        if (ShouldAttack(attackJoystickTouch))
        {
            if (!_isCharging && IsCoolDownPassed())
            {
                StartCharging();
            }

            if (_isCharging)
            {
                UpdateChargeColor();
            }

            return false;
        }

        if (!_isCharging)
        {
            return false;
        }

        float heldTime = Time.time - _chargeStartTime;
        StopCharging();

        if (heldTime < _chargedAttack.GetMinHoldTime())
        {
            return false;
        }

        GameObject releasedAttack = InstantiateDescreteAttack(attackPrefab);
        releasedAttack.GetComponent<ChargedAttack>().Release(_chargedAttack.GetChargeLevel(heldTime));
        return true;
    }

    private void StartCharging()
    {
        _isCharging = true;
        _chargeStartTime = Time.time;
        _halalitMovement.SetWeaponThrustFactor(_chargedAttack.GetChargingThrustFactor());
    }

    private void UpdateChargeColor()
    {
        int chargeLevel = _chargedAttack.GetChargeLevel(Time.time - _chargeStartTime);
        _spriteRenderer.color = _chargedAttack.GetChargeColor(_baseColor, chargeLevel);
    }

    private void StopCharging()
    {
        _isCharging = false;
        _halalitMovement.SetWeaponThrustFactor(1f);
        _spriteRenderer.color = _baseColor;
    }

    private bool TryConsecutiveAttack(Vector2 attackJoystickTouch, GameObject attackPrefab)
    {
        if (ShouldAttack(attackJoystickTouch))
        {
            if (_consecutiveAttack != null && _consecutiveAttack.gameObject.name.Replace("(Clone)", "") != attackPrefab.name) 
            {
                RemoveConsecutiveAttack();
            }

            if (_consecutiveAttack == null)
            {
                CreateConsecutiveAttack(attackPrefab);
            }

            if (!_shootingConsecutivaly)
            {
                StartConsecitiveAttack();
            }
            else
            {
                UpdateConsecitiveAttack();
            }

            return true;
        }

        if (!ShouldAttack(attackJoystickTouch) && _shootingConsecutivaly)
        {
            StopConsecutiveAttack();
            RemoveConsecutiveAttack();
        }

        return false;
    }

    private void RemoveConsecutiveAttack()
    {
        _consecutiveAttack = null;
    }

    private void CreateConsecutiveAttack(GameObject attackPrefab)
    {
        _consecutiveAttack = Instantiate(attackPrefab, transform.position, transform.rotation, _projectiles.transform).GetComponent<ConsecutiveAttack>();  
        _consecutiveAttack.transform.rotation = transform.rotation;
    }

    private void StartConsecitiveAttack()
    {
        _consecutiveAttack.StartConsecitiveAttack(transform.position, transform.rotation);
        _shootingConsecutivaly = true;
    }

    private void UpdateConsecitiveAttack()
    {
        _consecutiveAttack.UpdateConsecitiveAttack(transform.position, transform.rotation);
    }

    private void StopConsecutiveAttack()
    {
        _consecutiveAttack.StopConsecitiveAttack();
        _shootingConsecutivaly = false;
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
        _cooldownMultiplier -= properties[EventProperty.WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION];
        Debug.Log("Weapon stamina upgraded");
    } 
    #endregion

    #region Predicates
    private bool IsCoolDownPassed()
    {
        return Time.time >= _cooldownTime;
    }

    private bool ShouldAttack(Vector2 attackJoystickTouch)
    {
        return Utils.GetLengthOfLine(attackJoystickTouch.x, attackJoystickTouch.y) >= _attackJoystickEdge; // TODO: attackJoystickTouch.magnitude
    }


    private bool IsRelevantUpgradeEvent(ItemEventArguments arguments)
    {
        return _upgradeActions.ContainsKey(arguments.Name);
    }
    #endregion

#if UNITY_EDITOR
    internal float GetAttackJoystickEdge() 
    {
        return _attackJoystickEdge;
    }

    internal float GetCooldownInterval()
    {
        return _cooldownMultiplier;
    }

    internal void SetIsTesting(bool isTesting)
    {
        _isTesting = isTesting;
    }

    internal bool IsCharging()
    {
        return _isCharging;
    }
#endif
}