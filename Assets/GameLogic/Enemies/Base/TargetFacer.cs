using Assets.Models;
using Assets.Utils;
using UnityEngine;

// Mirror Shield (#200): turns the enemy to face what enemies aim at (the Halalit, or a Decoy Ship) at a constant rate.
public class TargetFacer : MonoBehaviour, IPausableEnemyBehaviour
{
    [SerializeField]
    private float _turnDegreesPerSecond = 90f;

    void FixedUpdate()
    {
        var targetRotation = Quaternion.LookRotation(Vector3.forward, EnemyTarget.GetDirection(transform));
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnDegreesPerSecond * Time.fixedDeltaTime);
    }
}
