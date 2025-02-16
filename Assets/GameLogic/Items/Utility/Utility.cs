using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public abstract class Utility : MonoBehaviour
{
    public ItemStats UtilityStats;
    protected ItemName _itemName;
    protected bool _isActive;
    protected float _activationEndTime;
    protected const float DURATION = 10f;

    protected virtual void Update()
    {
        if (_isActive && Time.time >= _activationEndTime)
        {
            DeactivateEffect();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
        {
            OnPlayerUtilityPickedUp(this, new(_itemName, UtilityStats.itemStats));
            Destroy(gameObject);
        }
    }

    protected void OnPlayerUtilityPickedUp(object initiator, ItemEventArguments arguments)
    {
        ItemEvent.Invoke(EventName.PLAYER_UTILITY_PICKUP, initiator, arguments);
    }

    public virtual void ActivateEffect()
    {
        _isActive = true;
        _activationEndTime = Time.time + DURATION;
    }

    protected virtual void DeactivateEffect()
    {
        _isActive = false;
    }
} 