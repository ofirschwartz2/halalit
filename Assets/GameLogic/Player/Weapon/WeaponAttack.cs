using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
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
    private bool _shootingConsecutivaly;

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
        _attackToggle = gameObject.GetComponentInParent<AttackToggle>();
        _cooldownTime = 0;
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
        if (!SceneManager.GetActiveScene().name.Contains("Testing"))
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

        return TryAttack(attackJoystickTouch, attackPrefab.Value, shotType);
    }

    private bool TryAttack(Vector2 attackJoystickTouch, GameObject attackPrefab, AttackShotType shotType)
    {
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

    private void InstantiateDescreteAttack(GameObject attackPrefab)
    {
        Instantiate(attackPrefab, transform.position, transform.rotation, _projectiles.transform);
        _cooldownTime = Time.time + _currentAttack.Value.Rate * _cooldownMultiplier; 
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
#endif
}