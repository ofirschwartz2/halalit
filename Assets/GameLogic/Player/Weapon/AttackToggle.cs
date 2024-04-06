using Assets.Enums;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
#endif

class AttackToggle : MonoBehaviour
{
    [SerializeField]
    private KeyValuePair<AttackName, AttackStats> _firstAttack;

    private AttackStats _firstAttackStats;
    private KeyValuePair<AttackName, GameObject> _currentAttack;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        _firstAttackStats = _firstAttack.Value;
    }

    private void SetEventListeners()
    {
        ItemEvent.PlayerAttackItemPickUp += NewAttackSwitch;
    }

    private void Start()
    {
        _currentAttack = new(_firstAttack.Key, AttacksBank.GetAttackPrefab(_firstAttack.Key));
        _currentAttack.Value.GetComponent<AttackBehaviour>().AttackStats = _firstAttackStats;
    }
    #endregion

    #region Accessors
    public KeyValuePair<AttackName, GameObject> GetCurrentAttack()
    {
        return _currentAttack;
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


#if UNITY_EDITOR
    internal
#else
    private
#endif
    void SetNewAttack(AttackName newAttackName, AttackStats attackStats)
    {
        _currentAttack = new(newAttackName, AttacksBank.GetAttackPrefab(newAttackName));
        AttackBehaviour attackBehaviour = _currentAttack.Value.GetComponent<AttackBehaviour>();
        attackBehaviour.AttackStats = attackStats;
        Debug.Log("New attack - " + newAttackName.ToString() + " loaded");
    }
#endregion
}

