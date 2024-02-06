using Assets.Enums;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemOptions : MonoBehaviour
{
    [SerializeField]
    private List<AttackOptions> _attacks;

    private void Awake()
    {
        foreach (AttackOptions attackOptions in _attacks)
        {
            attackOptions.InitRanks();
        }
    }

    public AttackOptions GetAttackPossibleStats(ItemName itemName)
    {
        return _attacks.Where(attackPossibleStats => attackPossibleStats.attackName == itemName).First();
    }
}
