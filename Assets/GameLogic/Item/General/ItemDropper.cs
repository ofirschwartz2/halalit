using Assets.Enums;
using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    private ItemsContainer _itemsContainer;

    #region Init
    private void Awake()
    {
        SetEventListeners();
    }

    private void Start()
    {
        _itemsContainer = GetComponentInParent<ItemsContainer>();
    }

    private void SetEventListeners()
    {
        DropEvent.ItemDrop += DropItem;
    }
    #endregion

    #region Item Dropping
    private void DropItem(object initiator, DropEventArguments arguments)
    {
        if (arguments.DropType == DropType.ITEM_DROP)
        {
            GameObject randomValidByRankItem = _itemsContainer.GetRandomValidItem(arguments.Luck);
            InstantiateItem(randomValidByRankItem, arguments.DropPosition, arguments.DropForce);
        }
    }

    public void InstantiateItem(GameObject itemPrefab, Vector2 position, Vector2 dropForce)
    {
        GameObject item = Instantiate(itemPrefab, position, Quaternion.identity);

        item.transform.SetParent(transform.parent);
        item.GetComponent<Rigidbody2D>().AddForceAtPosition(dropForce, item.transform.position);
    }
    #endregion
}
