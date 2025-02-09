using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class NitroFuel : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"NitroFuel: Collision with {other.gameObject.name}");
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            Debug.Log("NitroFuel: Picked up by Halalit");
            OnPlayerUtilityPickedUp();
            Destroy(gameObject);
        }
    }

    private void OnPlayerUtilityPickedUp()
    {
        var itemStats = GetComponent<ItemStats>();
        Debug.Log("NitroFuel: Invoking PLAYER_UTILITY_PICKUP event");
        ItemEvent.Invoke(EventName.PLAYER_UTILITY_PICKUP, this, new(ItemName.NITRO_FUEL, itemStats.itemStats));
    }
} 