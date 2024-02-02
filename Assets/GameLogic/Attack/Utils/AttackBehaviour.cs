using Assets.Enums;
using UnityEngine;

public class AttackBehaviour : MonoBehaviour, IHarmer
{
    public AttackShotType ShotType;
    public AttackStats AttackStats;

    public void Copy(AttackBehaviour attackBehaviour)
    {
        ShotType = attackBehaviour.ShotType;
        AttackStats = attackBehaviour.AttackStats;
    }

    public int GetHarm()
    {
       return AttackStats.Power;
    }
}