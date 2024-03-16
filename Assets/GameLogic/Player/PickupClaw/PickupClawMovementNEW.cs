using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawMovementNEW : MonoBehaviour
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

    internal void Move(PickupClawStateENEW state)
    {
        MoveTowardsTarget(state);
        TryRotateAccordingToTarget(state);
    }

    #region Movement
    private void MoveTowardsTarget(PickupClawStateENEW state)
    {
        if (_target == null)
        {
            throw new System.Exception("Target is null");
        }

        SetVelocity(state);
    }

    private void SetVelocity(PickupClawStateENEW state)
    {
        var direction = _target.transform.position - transform.position;
        var velocity = direction.normalized * GetSpeed(state);
        _rigidBody.velocity = velocity;
    }
    #endregion

    #region Rotation
    private void TryRotateAccordingToTarget(PickupClawStateENEW state)
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
    private float GetSpeed(PickupClawStateENEW state)
    {
        switch (state)
        {
            case PickupClawStateENEW.MOVING_TO_TARGET:
                return _speedToItem;

            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET:
                return _speedToHalalitWithItem;

            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET:
                return _speedToHalalitWithoutItem;

            default:
                throw new System.Exception("Invalid state");
        }
    }

    private Vector2 GetMovementDirection(PickupClawStateENEW state)
    {
        switch (state)
        {
            case PickupClawStateENEW.MOVING_TO_TARGET:
                return (_target.transform.position - transform.position).normalized;

            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITH_TARGET:
            case PickupClawStateENEW.RETURNING_TO_HALALIT_WITHOUT_TARGET:
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