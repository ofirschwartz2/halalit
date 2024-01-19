using Assets.Enums;
using UnityEngine;

public class AttackBehaviour : MonoBehaviour, IHarmer
{
    public AttackShotType ShotType;
    public AttackStats AttackStats;

    public int GetHarm()
    {
       return AttackStats.Power;
    }
}