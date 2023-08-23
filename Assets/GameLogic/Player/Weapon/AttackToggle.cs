using Assets.Enums;
using System;
using UnityEngine;

class AttackToggle : MonoBehaviour
{
    [SerializeField]
    private AttackName _firstAttack;

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
        SetNewAttack(attackName);
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
        // TODO: implement...
    }

    private void SetNewAttack(AttackName newAttackName)
    {
        if (_currentAttack.Key != newAttackName)
        {
            _currentAttack = new(newAttackName, AttacksBank.GetAttackPrefab(newAttackName));
            Debug.Log("New attack - " + newAttackName.ToString() + " loaded");
        }
    }
    #endregion
}

