using Assets.Animators;
using Assets.Enums;
using Assets.Utils;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawStateMachineNEW : MonoBehaviour
{
    [SerializeField]
    private PickupClawState _pickupClawState;
    [SerializeField]
    private PickupClawShooter _pickupClawShooter;
    [SerializeField]
    private PickupClawGrabberNEW _pickupClawGrabberNEW;
    [SerializeField]
    private PickupClawRetractorNEW _pickupClawRetractorNEW;
    [SerializeField]
    private PickupClawAnimator _pickupClawAnimator;
    [SerializeField]
    private PickupClawMovementNEW _pickupClawMovementNEW;
    [SerializeField]
    private float _pickupClawManeuverRadius;

    private PickupClawStateENEW _state;
    private GameObject _item;
    private GameObject _halalit;

    void Start()
    {
        _state = PickupClawStateENEW.MOVING_TO_TARGET;
        _pickupClawMovementNEW.SetTarget(_item);
        _pickupClawMovementNEW.SetFacingTarget(true);
    }

    void FixedUpdate()
    {
        switch (_state) 
        {
            case PickupClawStateENEW.MOVING_TO_TARGET:
                _pickupClawMovementNEW.MoveTowardsTarget();
                _pickupClawMovementNEW.TryRotateInRelationToTarget();
                TryChangeToGrabbing();
                break;

            case PickupClawStateENEW.GRABBING:
                _pickupClawGrabberNEW.GrabTarget(_item);
                if (!TryChangeToReturningToHalalitWithTarget()) 
                {
                    TryChangeToReturningToHalalitWithoutTarget();
                }
                break;

            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET:
            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                _pickupClawMovementNEW.MoveTowardsTarget();
                _pickupClawMovementNEW.TryRotateInRelationToTarget();
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
        _pickupClawRetractorNEW.SetSpeed(true);
        _pickupClawMovementNEW.SetFacingTarget(false);
        _pickupClawMovementNEW.SetTarget(_halalit);
    }

    private void ChangeToReturningToHalalitWithoutTarget()
    {
        _state = PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET;
        _pickupClawRetractorNEW.SetSpeed(false);
        _pickupClawMovementNEW.SetFacingTarget(false);
        _pickupClawMovementNEW.SetTarget(_halalit);
    }

    private void TryDie()
    {
        if (_pickupClawMovementNEW.IsOnTarget())
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Init
    public void SetTarget(GameObject target)
    {
        _item = target;
    }

    public void SetHalalit(GameObject halalit)
    {
        _halalit = halalit;
    }
    #endregion

    #region Predicates
    private bool IsClawOnTarget()
    {
        return Utils.Are2VectorsAlmostEqual(_item.transform.position, transform.position);
    }

    private bool ShouldClawRetract() 
    {
        return Utils.GetDistanceBetweenTwoPoints(Utils.GetHalalitPosition(), transform.position) > _pickupClawManeuverRadius;
    }
    #endregion

}
