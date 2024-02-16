using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupClawMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private PickupClawState _pickupClawState;
    [SerializeField]
    private float _ropeLength;
    [SerializeField]
    private float _rotationToHalalitMultiplier;
    [SerializeField]
    private float _rotationToItemMultiplier;
    [SerializeField]
    private float _moveToItemMultiplier;
    [SerializeField]
    private float _velocityMultiplier;
    [SerializeField]
    private float _rotationMinProximity; 
    [SerializeField]
    private float _positionMinProximity;
    [SerializeField]
    private float _dragAtFullRopeSpeed;

    private Vector2 _pickupClawDirection;
    private Vector2 _halalitLastFramePosition;
    private float _initialRotation;
    private bool _perfectRotationToHalalit;
    private Dictionary<ItemName, Action<Dictionary<EventProperty, float>>> _upgradeActions;

    #region Init
    private void Awake()
    {
        SetEventListeners();
        SetUpgradeActions();
    }

    private void SetEventListeners()
    {
        ItemEvent.PlayerUpgradePickUp += UpgradeClaw;
    }

    private void SetUpgradeActions()
    {
        _upgradeActions = new()
        {
            { ItemName.CLAW_RANGE, UpgradeClawRange },
            { ItemName.CLAW_AGILITY, UpgradeClawSpeed },
        };
    }

    void Start()
    {
        _initialRotation = transform.rotation.eulerAngles.z;
        _halalitLastFramePosition = _halalit.transform.position;
    }
    #endregion

    #region Events actions
    private void UpgradeClaw(object initiator, ItemEventArguments arguments)
    {
        if (IsRelevantUpgradeEvent(arguments))
        {
            _upgradeActions[arguments.Name].Invoke(arguments.Properties);
        }
    }

    private bool IsRelevantUpgradeEvent(ItemEventArguments arguments)
    {
        return _upgradeActions.ContainsKey(arguments.Name);
    }

    private void UpgradeClawRange(Dictionary<EventProperty, float> properties)
    {
        _ropeLength += properties[EventProperty.CLAW_RANGE_ADDITION];
        Debug.Log("Claw range upgraded");
    }

    private void UpgradeClawSpeed(Dictionary<EventProperty, float> properties)
    {
        _velocityMultiplier += properties[EventProperty.CLAW_SPEED_MULTIPLIER_ADDITION];
        Debug.Log("Claw agility upgraded");
    }
    #endregion

    #region Public movement by states
    public void Move(Vector2 goal)
    {
        UpdatePickupClawDirection(goal);

        if (_pickupClawState.Value == PickupClawStateE.MOVING_FORWARD && AtFullRopeLength())
        {
            DragAtFullRopeLength();
        }

        if (_pickupClawState.Value == PickupClawStateE.MOVING_FORWARD || _perfectRotationToHalalit)
        {
            RotateToPickupClawDirectionInstantly();
        }
        else if (!_perfectRotationToHalalit)
        {
            RotateToDirectionSlowly(_pickupClawDirection, true);
        }

        UpdateVelocityByPickupClawDirection();
        SaveHalalitLastFramePosition();
    }

    public void MoveToItem(Vector3 itemPosition)
    {
        Vector2 directionToItem = Utils.GetDirectionVector(transform.position, itemPosition);

        MoveCloserToItem(itemPosition);
        RotateToDirectionSlowly(directionToItem);
    }

    public bool AtFullRopeLength()
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, _halalit.transform.position) >= _ropeLength;
    }

    public bool ReachGoal(Vector2 goal)
    {
        return Utils.GetDistanceBetweenTwoPoints(transform.position, goal) <= _positionMinProximity;
    }

    public void SetPerfectRotationToHalalit(bool perfectRotationToHalalit)
    {
        _perfectRotationToHalalit = perfectRotationToHalalit;
    }
    #endregion

    #region Movement helpers
    private void SaveHalalitLastFramePosition()
    {
        _halalitLastFramePosition = _halalit.transform.position;
    }

    private void UpdatePickupClawDirection(Vector2 goal)
    {
        float deltaX = Math.Abs(transform.position.x - goal.x);
        float deltaY = Math.Abs(transform.position.y - goal.y);

        float relativeDeltaX = transform.position.x < goal.x ? deltaX : deltaX * -1;
        float relativeDeltaY = transform.position.y < goal.y ? deltaY : deltaY * -1;

        float shootDirectionMagnitude = new Vector2(relativeDeltaX, relativeDeltaY).magnitude;

        _pickupClawDirection = new Vector2(relativeDeltaX / shootDirectionMagnitude, relativeDeltaY / shootDirectionMagnitude);
    }

    private void DragAtFullRopeLength()
    {
        float halalitDeltaX = (_halalit.transform.position.x - _halalitLastFramePosition.x);
        float halalitDeltaY = (_halalit.transform.position.y - _halalitLastFramePosition.y);
        Vector3 newPosition = new(transform.position.x + halalitDeltaX, transform.position.y + halalitDeltaY, transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, newPosition, _dragAtFullRopeSpeed);
    }

    private void RotateToPickupClawDirectionInstantly()
    {
        float pickupClawDirectionAngle = Utils.Vector2ToDegrees(_pickupClawDirection.x, _pickupClawDirection.y);
        float pickupClawRotation = transform.rotation.eulerAngles.z - _initialRotation; ;

        if (_pickupClawState.Value == PickupClawStateE.MOVING_BACKWARD)
        {
            pickupClawRotation += 180;
        }

        transform.Rotate(new Vector3(0, 0, pickupClawDirectionAngle - pickupClawRotation));
    }

    private void RotateToDirectionSlowly(Vector2 goal, bool oposite = false)
    {
        float pickupClawRotation = !oposite ?
            Utils.AngleNormalizationBy360(transform.rotation.eulerAngles.z - _initialRotation) :
            Utils.AngleNormalizationBy360(transform.rotation.eulerAngles.z - _initialRotation + 180);
        float directionRotation = Utils.AngleNormalizationBy360(Utils.Vector2ToDegrees(goal.x, goal.y));
        float rotationAntiClockwiseDeference = Utils.AngleNormalizationBy360(directionRotation - pickupClawRotation);
        float rotationClockwiseDeference = 360 - rotationAntiClockwiseDeference;

        if (rotationAntiClockwiseDeference < _rotationMinProximity || rotationClockwiseDeference < _rotationMinProximity)
        {
            _perfectRotationToHalalit = true;
        }
        else if (rotationAntiClockwiseDeference <= rotationClockwiseDeference)
        {
            transform.Rotate(0, 0, _rotationToItemMultiplier);
        }
        else
        {
            transform.Rotate(0, 0, -_rotationToItemMultiplier);
        }
    }

    private void UpdateVelocityByPickupClawDirection()
    {
        float horizontalVelocity = _pickupClawDirection.x * _velocityMultiplier;
        float verticalVelocity = _pickupClawDirection.y * _velocityMultiplier;

        _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
    }

    private void MoveCloserToItem(Vector3 itemPosition)
    {
        Vector3 directionToItem = Utils.GetDirectionVector(transform.position, itemPosition);
        transform.position += _moveToItemMultiplier * Time.deltaTime * directionToItem;
    }
    #endregion
}