using Assets.Utils;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawMovementNEW : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _rotationToTargetSpeed;
    [SerializeField]
    private float _rotationFromHalalitSpeed;
    [SerializeField]
    private float _radiusToStartGrabbing;
    [SerializeField]
    private float _rotationDelta;

    private GameObject _target;
    private bool _facingTarget;

    internal void MoveTowardsTarget()
    {
        if (_target == null)
        {
            throw new System.Exception("Target is null");
        }

        Vector2 direction = _target.transform.position - transform.position;
        Vector2 velocity = direction.normalized * _speed;
        _rigidBody.velocity = velocity;
    }

    internal void TryRotate()
    {
        var direction = _facingTarget ?
            _target.transform.position - transform.position
            :
            transform.position - _target.transform.position;

        var targetDirectionInDegrees = Utils.Vector2ToDegrees(direction);
        var clawDirectionInDegrees = Utils.GetAngleFromQuaternion(transform.rotation);

        if (ShouldRotate(targetDirectionInDegrees, clawDirectionInDegrees))
        {
            Rotate(targetDirectionInDegrees);
        }
    }

    private void Rotate(float targetDirectionInDegrees)
    {
        bool shouldRotateClockwise = Utils.IsCloserClockwise(Utils.GetAngleFromQuaternion(transform.rotation), targetDirectionInDegrees);

        if (shouldRotateClockwise)
        {
            transform.rotation = Utils.GetRotationPlusAngle(transform.rotation, _rotationSpeed); // TODO: check if flip is needed
        }
        else
        {
            transform.rotation = Utils.GetRotationPlusAngle(transform.rotation, -_rotationSpeed); // TODO: check if flip is needed
        }
    }

    private bool ShouldRotate(float targetDirectionInDegrees, float clawDirectionInDegrees)
    {
        return Math.Abs(targetDirectionInDegrees - clawDirectionInDegrees) > _rotationDelta;
    }

    internal bool IsOnTarget()
    {
        return Utils.Are2VectorsAlmostEqual(_target.transform.position, transform.position);
    }

    #region Setters
    internal void SetTarget(GameObject target)
    {
        _target = target;
    }

    internal void SetFacingTarget(bool facingTarget)
    {
        _facingTarget = facingTarget;
    }
    #endregion

}