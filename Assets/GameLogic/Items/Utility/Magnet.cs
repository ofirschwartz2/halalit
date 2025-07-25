using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class Magnet : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Magnet: Collision with {other.gameObject.name}");
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            Debug.Log("Magnet: Picked up by Halalit");
            OnPlayerUtilityPickedUp();
            Destroy(gameObject);
        }
    }

    private void OnPlayerUtilityPickedUp()
    {
        var itemStats = GetComponent<ItemStats>();
        Debug.Log("Magnet: Invoking PLAYER_UTILITY_PICKUP event");
        ItemEvent.Invoke(EventName.PLAYER_UTILITY_PICKUP, this, new(ItemName.MAGNET, itemStats.itemStats));
    }
} 