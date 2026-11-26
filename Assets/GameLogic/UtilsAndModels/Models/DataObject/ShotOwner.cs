using UnityEngine;

// Grudge (#216): the enemy that fired a shot. Added to the shot at runtime by whoever fires it.
// Player shots have no owner.
public class ShotOwner : MonoBehaviour
{
    private Transform _owner;

    public static void Mark(GameObject shot, Transform owner)
    {
        var shotOwner = shot.GetComponent<ShotOwner>();
        if (shotOwner == null)
        {
            shotOwner = shot.AddComponent<ShotOwner>();
        }

        shotOwner._owner = owner;
    }

    public Transform GetOwner()
    {
        return _owner;
    }
}
