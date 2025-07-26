using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class PullingMagnet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUtilityPickedUp();
            Destroy(gameObject);
        }
    }

    private void OnPlayerUtilityPickedUp()
    {
        var itemStats = GetComponent<ItemStats>();
        ItemEvent.Invoke(EventName.PLAYER_UTILITY_PICKUP, this, new(ItemName.PULLING_MAGNET, itemStats.itemStats));
    }
} 