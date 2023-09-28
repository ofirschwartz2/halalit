using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class EnemySharedBehavior : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }
}

