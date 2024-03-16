using Assets.Animators;
using Assets.Enums;
using Assets.Utils;
using System.Net;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawStateMachineNEW : MonoBehaviour
{

    [SerializeField]
    private PickupClawGrabberNEW _pickupClawGrabberNEW;
    [SerializeField]
    private PickupClawAnimator _pickupClawAnimator;
    [SerializeField]
    private PickupClawMovementNEW _pickupClawMovementNEW;
    [SerializeField]
    private float _pickupClawManeuverRadius;
    [SerializeField]
    private float _isOnTargetDelta;
    [SerializeField]
    private float _returningToHalalitOpacity;

    private PickupClawStateENEW _state;
    private GameObject _item;
    private GameObject _halalit;

    void Start()
    {
        _state = PickupClawStateENEW.MOVING_TO_TARGET;
        _pickupClawMovementNEW.SetTarget(_item);
    }

    void FixedUpdate()
    {
        switch (_state) 
        {
            case PickupClawStateENEW.MOVING_TO_TARGET:
                _pickupClawMovementNEW.Move(_state);
                TryChangeToGrabbing();
                break;

            case PickupClawStateENEW.GRABBING:
                _pickupClawGrabberNEW.GrabTarget(_item);
                TryChangeToReturningToHalalit();
                break;

            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET:
            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                _pickupClawMovementNEW.Move(_state);
                TryChangeToReturningToHalalitWithoutTarget();
                TryDie();
                break;
        }
        
    }

    #region State Changes

    private bool TryChangeToReturningToHalalitWithTarget()
    {
        if (IsClawOnTarget())
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
        var itemToGrab = _pickupClawGrabberNEW.TryGetItemToGrab();
        if (itemToGrab != null)
        {
            ChangeToGrabbing(itemToGrab);
        }
    }

    private void ChangeToGrabbing(GameObject itemToGrab)
    {
        _state = PickupClawStateENEW.GRABBING;
        _item = itemToGrab;
    }

    private void ChangeToReturningToHalalitWithTarget()
    {
        _item.GetComponent<ItemMovement>().Grabbed(transform);
        _state = PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET;
        _pickupClawMovementNEW.SetTarget(_halalit);
    }

    private void ChangeToReturningToHalalitWithoutTarget()
    {
        if (_state == PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET)
        {
            _item.GetComponent<ItemMovement>().UnGrabbed();
        }
        _state = PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET;
        _pickupClawMovementNEW.SetTarget(_halalit);

        Utils.ChangeOpacity(GetComponent<Renderer>(), _returningToHalalitOpacity);

    }

    private void TryDie()
    {
        if (_pickupClawMovementNEW.IsOnTarget())
        {
            Destroy(gameObject);
        }
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
    private bool IsClawOnTarget()
    {
        return Utils.Are2VectorsAlmostEqual(_item.transform.position, transform.position, _isOnTargetDelta);
    }

    private bool ShouldClawRetract() 
    {
        return Utils.GetDistanceBetweenTwoPoints(Utils.GetHalalitPosition(), transform.position) > _pickupClawManeuverRadius;
    }
    #endregion

}
