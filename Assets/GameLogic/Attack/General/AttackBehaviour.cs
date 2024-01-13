using Assets.Enums;
using System;
using UnityEngine;

[Serializable]
public class AttackBehaviour : MonoBehaviour, IHarmer
{
    public AttackShotType ShotType;
    public AttackStats AttackStats;

    public int GetHarm()
    {
       return AttackStats.Power;
    }
}