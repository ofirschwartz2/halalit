using Assets.Utils;
using UnityEngine;

public class DecoyShip : MonoBehaviour
{
    private void Awake()
    {
        EnemyTarget.AddDecoy(transform);
    }

    private void OnDestroy()
    {
        EnemyTarget.RemoveDecoy(transform);
    }
}
