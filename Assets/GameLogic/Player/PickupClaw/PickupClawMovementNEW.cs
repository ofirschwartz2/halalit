using Assets.Utils;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PickupClawMovementNEW : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _rotationToTargetSpeed;
    [SerializeField]
    private float _rotationFromHalalitSpeed;
    [SerializeField]
    private float _radiusToStartGrabbing;

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

    internal void Rotate() 
    {
        Vector2 direction = _facingTarget? 
            _target.transform.position - transform.position 
            :
            transform.position - _target.transform.position;

        direction = direction.normalized;

        if (_facingTarget)
        {
            Utils.GetRotationPlusAngle(transform.rotation, _rotationToTargetSpeed);
        }
        else 
        {
        }
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