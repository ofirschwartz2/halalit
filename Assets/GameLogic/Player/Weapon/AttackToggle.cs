using Assets.Enums;
using System;
using UnityEngine;

class AttackToggle : MonoBehaviour
{
    [SerializeField]
    private AttackName _firstAttack;
    [SerializeField]
    private AttackStats _firstAttackStats;

    private KeyValuePair<AttackName, GameObject> _currentAttack;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void SetEventListeners()
    {
        ItemEvent.PlayerAttackItemPickUp += NewAttackSwitch;
    }

    private void Start()
    {
        _currentAttack = new(_firstAttack, AttacksBank.GetAttackPrefab(_firstAttack));
        _currentAttack.Value.GetComponent<AttackBehaviour>().AttackStats = _firstAttackStats;
    }
    #endregion

    #region Accessors
    public GameObject GetAttackPrefab()
    {
        return _currentAttack.Value;
    }
    #endregion

    #region Event Actions
    private void NewAttackSwitch(object initiator, ItemEventArguments arguments)
    {
        AttackName attackName = GetAttackName(arguments.Name);

        RemoveOldAttack();
        SetNewAttack(attackName, (AttackStats)arguments.ItemStats);
    }

    private AttackName GetAttackName(ItemName name)
    {
        if (Enum.IsDefined(typeof(AttackName), name.ToString()))
        {
            return (AttackName)Enum.Parse(typeof(AttackName), name.ToString());
        }

        throw new Exception("Item name: " + name.ToString() + " is not a valid attack name");
    }

    private void RemoveOldAttack()
    {
        // TODO (dev): implement...
    }

    private void SetNewAttack(AttackName newAttackName, AttackStats attackStats)
    {
        if (_currentAttack.Key != newAttackName)
        {
            _currentAttack = new(newAttackName, AttacksBank.GetAttackPrefab(newAttackName));
            AttackBehaviour attackBehaviour = _currentAttack.Value.GetComponent<AttackBehaviour>();
            attackBehaviour.AttackStats = attackStats;
            Debug.Log("New attack - " + newAttackName.ToString() + " loaded");
        }
    }
    #endregion
}

