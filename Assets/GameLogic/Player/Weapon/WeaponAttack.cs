using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private GameObject _projectiles;
    [SerializeField]
    private float _cooldownInterval;
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

    #region Logic
    void FixedUpdate()
    {
        HumbleFixedUpdate(new Vector2(_attackJoystick.Vertical, _attackJoystick.Horizontal));
    }

    public void HumbleFixedUpdate(Vector2 attackJoystickTouch)
    {
        // TODO (refactor): this should be via event raise, not in update
        KeyValuePair<AttackName, GameObject> attackPrefab = _attackToggle.GetCurrentAttack();
        _currentAttack.Key = attackPrefab.Key;
        _currentAttack.Value = attackPrefab.Value.GetComponent<AttackBehaviour>().AttackStats;
        AttackShotType shotType = attackPrefab.Value.GetComponent<AttackBehaviour>().ShotType;
        // TODO (refactor): this should be via event raise, not in update

        TryAttack(attackJoystickTouch, attackPrefab.Value, shotType);
    }

    private void TryAttack(Vector2 attackJoystickTouch, GameObject attackPrefab, AttackShotType shotType)
    {
        if (shotType == AttackShotType.DESCRETE)
        {
            TryDescreteAttack(attackJoystickTouch, attackPrefab);
        }
        else if (shotType == AttackShotType.CONSECUTIVE)
        {
            TryConsecutiveAttack(attackJoystickTouch, attackPrefab);
        }
    }

    private void TryDescreteAttack(Vector2 attackJoystickTouch, GameObject attackPrefab)
    {
        if (ShouldAttack(attackJoystickTouch) && IsCoolDownPassed())
        {
            if (_consecutiveAttack != null)
            {
                RemoveConsecutiveAttack();
            }

            InstantiateDescreteAttack(attackPrefab);
        }
    }

    private void InstantiateDescreteAttack(GameObject attackPrefab)
    {
        Instantiate(attackPrefab, transform.position, transform.rotation, _projectiles.transform);
        _cooldownTime = Time.time + _cooldownInterval; 
    }

    private void TryConsecutiveAttack(Vector2 attackJoystickTouch, GameObject attackPrefab)
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
        }

        if (!ShouldAttack(attackJoystickTouch) && _shootingConsecutivaly)
        {
            StopConsecutiveAttack();
            RemoveConsecutiveAttack();
        }
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
        _cooldownInterval -= properties[EventProperty.WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION];
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

    public float GetAttackJoystickEdge() 
    {
        return _attackJoystickEdge;
    }
}
