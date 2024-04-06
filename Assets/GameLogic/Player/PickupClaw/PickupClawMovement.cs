using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speedToItem;
    [SerializeField]
    private float _speedToHalalitWithItem;
    [SerializeField]
    private float _speedToHalalitWithoutItem;
    [SerializeField]
    private float _rotationSpeed;

    [SerializeField]
    private float _rotationDelta;

    private GameObject _target;

    internal void Move(PickupClawState state)
    {
        if (_target == null)
        {
            Debug.Log("Target is null");
            return;
        }

        MoveTowardsTarget(state);
        TryRotateAccordingToTarget(state);
    }

    #region Movement
    private void MoveTowardsTarget(PickupClawState state)
    {
        SetVelocity(state);
    }

    private void SetVelocity(PickupClawState state)
    {
        var direction = _target.transform.position - transform.position;
        var velocity = direction.normalized * GetSpeed(state);
        _rigidBody.velocity = velocity;
    }
    #endregion

    #region Rotation
    private void TryRotateAccordingToTarget(Assets.Enums.PickupClawState state)
    {
        var direction = GetMovementDirection(state);
        
        var targetDirectionInDegrees = Utils.Vector2ToDegrees(direction);
        var clawDirectionInDegrees = Utils.QuaternionToDegrees(transform.rotation);

        if (ShouldRotate(targetDirectionInDegrees, clawDirectionInDegrees))
        {
            Rotate(targetDirectionInDegrees);
        }
    }

    private void Rotate(float targetDirectionInDegrees)
    {
        bool shouldRotateClockwise = Utils.IsCloserClockwise(Utils.QuaternionToDegrees(transform.rotation), targetDirectionInDegrees);

        if (shouldRotateClockwise)
        {
            transform.rotation = Utils.GetRotationPlusAngle(transform.rotation, -_rotationSpeed);
        }
        else
        {
            transform.rotation = Utils.GetRotationPlusAngle(transform.rotation, _rotationSpeed);
        }
    }
    #endregion

    #region Setters
    internal void SetTarget(GameObject target)
    {
        _target = target;
    }
    #endregion

    #region Getters
    private float GetSpeed(Assets.Enums.PickupClawState state)
    {
        switch (state)
        {
            case Assets.Enums.PickupClawState.MOVING_TO_TARGET:
                return _speedToItem;

            case Assets.Enums.PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET:
                return _speedToHalalitWithItem;

            case Assets.Enums.PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                return _speedToHalalitWithoutItem;

            default:
                throw new System.Exception("Invalid state");
        }
    }

    private Vector2 GetMovementDirection(Assets.Enums.PickupClawState state)
    {
        switch (state)
        {
            case Assets.Enums.PickupClawState.MOVING_TO_TARGET:
                return (_target.transform.position - transform.position).normalized;

            case Assets.Enums.PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET:
            case Assets.Enums.PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                return (transform.position - _target.transform.position).normalized;

            default:
                throw new System.Exception("Invalid state");
        }
    }
    #endregion

    #region Predicates
    private bool ShouldRotate(float targetDirectionInDegrees, float clawDirectionInDegrees)
    {
        return Math.Abs(targetDirectionInDegrees - clawDirectionInDegrees) > _rotationDelta;
    }
    #endregion

}