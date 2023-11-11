using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class PickupClawGrabber : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private PickupClawMovement _pickupClawMovement;
    [SerializeField]
    private PickupClawState _pickupClawState;
    [SerializeField]
    private float _grabDelay;
    
    private float _grabDelayTimer;
    private GameObject _item;

    public void Grab()
    {
        _grabDelayTimer += Time.deltaTime;
        _pickupClawMovement.MoveToItem(_item.transform.position);

        if (_grabDelayTimer >= _grabDelay)
        {
            _item.GetComponent<ItemMovement>().Grabbed(transform);
            _pickupClawState.Value = PickupClawStateE.MOVING_BACKWARD;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ITEM.GetDescription()) && _pickupClawState.Value == PickupClawStateE.MOVING_FORWARD)
        {
            _pickupClawState.Value = PickupClawStateE.GRABBING;
            _grabDelayTimer = 0;
            _rigidBody.velocity = Vector2.zero;
            _item = other.gameObject;
        }
    }
}