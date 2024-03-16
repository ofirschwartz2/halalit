using Assets.Animators;
using Assets.Enums;
using Assets.Utils;
using System.Net;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawStateMachine : MonoBehaviour
{

    [SerializeField]
    private PickupClawGrabber _pickupClawGrabber;
    [SerializeField]
    private PickupClawAnimator _pickupClawAnimator;
    [SerializeField]
    private PickupClawMovement _pickupClawMovement;
    [SerializeField]
    private float _pickupClawManeuverRadius;
    [SerializeField]
    private float _isOnTargetDelta;
    [SerializeField]
    private float _returningToHalalitOpacity;

    private PickupClawState _state;
    private GameObject _item;
    private GameObject _halalit;

    void Start()
    {
        _state = PickupClawState.MOVING_TO_TARGET;
        _pickupClawMovement.SetTarget(_item);
    }

    void FixedUpdate()
    {
        switch (_state) 
        {
            case PickupClawState.MOVING_TO_TARGET:
                _pickupClawMovement.Move(_state);
                TryChangeToGrabbing();
                break;

            case PickupClawState.GRABBING:
                _pickupClawGrabber.GrabTarget(_item);
                TryChangeToReturningToHalalit();
                break;

            case PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET:
            case PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                _pickupClawMovement.Move(_state);
                TryChangeToReturningToHalalitWithoutTarget();
                TryDie(_state);
                break;
        }
        
    }

    #region State Changes

    private bool TryChangeToReturningToHalalitWithTarget()
    {
        if (IsClawOnTarget(_item))
        {
            ChangeToReturningToHalalitWithTarget();
            return true;
        }
        return false;
    }

    private void TryChangeToReturningToHalalitWithoutTarget()
    {
        if (ShouldClawRetract())
        {
            ChangeToReturningToHalalitWithoutTarget();
        }
    }

    private void TryChangeToGrabbing() 
    {
        var itemToGrab = _pickupClawGrabber.TryGetItemToGrab();
        if (itemToGrab != null)
        {
            ChangeToGrabbing(itemToGrab);
        }
    }

    private void ChangeToGrabbing(GameObject itemToGrab)
    {
        _state = PickupClawState.GRABBING;
        _item = itemToGrab;
    }

    private void ChangeToReturningToHalalitWithTarget()
    {
        _item.GetComponent<ItemMovement>().Grabbed(transform);
        _state = PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET;
        _pickupClawMovement.SetTarget(_halalit);
    }

    private void ChangeToReturningToHalalitWithoutTarget()
    {
        if (_state == PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET)
        {
            _item.GetComponent<ItemMovement>().UnGrabbed();
        }
        _state = PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET;
        _pickupClawMovement.SetTarget(_halalit);

        Utils.ChangeOpacity(GetComponent<Renderer>(), _returningToHalalitOpacity);

    }

    private void TryDie(Assets.Enums.PickupClawState _state)
    {
        if (IsClawOnTarget(_halalit) || WasItemCaught(_state))
        {
            Destroy(gameObject);
        } 
    }

    private bool WasItemCaught(Assets.Enums.PickupClawState _state)
    {
        return 
            _state == PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET &&
            _item == null;
    }

    private void TryChangeToReturningToHalalit()
    {
        if (!TryChangeToReturningToHalalitWithTarget())
        {
            TryChangeToReturningToHalalitWithoutTarget();
        }
    }

    #endregion

    #region Init
    public void SetTarget(GameObject target)
    {
        _item = target;
        transform.rotation = Utils.GetRorationOutwards(transform.position, target.transform.position);
    }

    public void SetHalalit(GameObject halalit)
    {
        _halalit = halalit;
    }
    #endregion

    #region Predicates
    private bool IsClawOnTarget(GameObject target)
    {
        return Utils.Are2VectorsAlmostEqual(target.transform.position, transform.position, _isOnTargetDelta);
    }

    private bool ShouldClawRetract() 
    {
        return Utils.GetDistanceBetweenTwoPoints(Utils.GetHalalitPosition(), transform.position) > _pickupClawManeuverRadius;
    }
    #endregion

}
