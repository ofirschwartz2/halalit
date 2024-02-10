using Assets.Enums;
using System.Collections.Generic;
using UnityEngine;

public class AttacksBank : MonoBehaviour
{
    [SerializeField]
    private List<KeyValuePair<AttackName,GameObject>> _shortRangeAttacks;
    [SerializeField]
    private List<KeyValuePair<AttackName, GameObject>> _middleRangeAttacks;
    [SerializeField]
    private List<KeyValuePair<AttackName, GameObject>> _longRangeAttacks;

    private static Dictionary<AttackName, GameObject> _allAttacksByName;

    #region Init
    private void Awake()
    {
        _allAttacksByName = new();

        AddToAllAttacksDictionary(_shortRangeAttacks);
        AddToAllAttacksDictionary(_middleRangeAttacks);
        AddToAllAttacksDictionary(_longRangeAttacks);
    }

    private void AddToAllAttacksDictionary(List<KeyValuePair<AttackName, GameObject>> attacks)
    {
        foreach (KeyValuePair<AttackName, GameObject> attack in attacks)
        {
            _allAttacksByName.Add(attack.Key, attack.Value);
        }
    }
    #endregion

    #region Accessors
    public static GameObject GetAttackPrefab(AttackName attackName)
    {
        return _allAttacksByName[attackName];
    }
    #endregion
}