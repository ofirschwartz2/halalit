using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class HalalitMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private Joystick _joystick;
    [SerializeField]
    private float _forceMultiplier;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private float _knockbackMultiplier;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _knockBackCooldownInterval;
    
    private float _knockBackCooldownTime;

    void FixedUpdate()
    {
        if (IsKnockBackCooledDown())
        {
            RotateByMovementJoystick();
            MoveInRotateDirection();
        }  
    }

    #region Moving 
    private void RotateByMovementJoystick()
    {
        if (IsMovementInput())
        {
            float joystickAngle = Utils.Vector2ToDegree(_joystick.Horizontal, _joystick.Vertical);
            float rotationZ = transform.rotation.eulerAngles.z;

            float normalizedJoystickAngle = Utils.AngleNormalizationBy360(joystickAngle);
            float normalizedRotationZ = Utils.AngleNormalizationBy360(rotationZ);

            float deltaAngle = normalizedJoystickAngle - normalizedRotationZ;
            float shorterDeltaAngle = Utils.GetNormalizedAngleBy360(deltaAngle);

            transform.Rotate(_rotationSpeed * Time.deltaTime * new Vector3(0, 0, shorterDeltaAngle));
        }
    }

    private void MoveInRotateDirection()
    {
        if (IsMovementInput() && Utils.IsUnderSpeedLimit(_rigidBody.velocity, _speedLimit))
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalForce = Utils.GetDirectionalForce(direction.x, Math.Abs(_joystick.Horizontal), _forceMultiplier);
            float verticalForce = Utils.GetDirectionalForce(direction.y, Math.Abs(_joystick.Vertical), _forceMultiplier);

            _rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
        }
    }
    #endregion

    #region Knockback
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            KnockBack(other);
        }
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 knockbackDirection = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        float knockBackSpeed = otherCollider2D.GetComponent<Rigidbody2D>().velocity.magnitude * _knockbackMultiplier;
        _rigidBody.AddForce(knockbackDirection * knockBackSpeed, ForceMode2D.Impulse);
        
        _knockBackCooldownTime = Time.time + _knockBackCooldownInterval;
    }
    #endregion

    #region Predicates
    private bool IsMovementInput()
    {
        return IsXInput() || IsYInput();
    }

    private bool IsXInput()
    {
        return _joystick.Horizontal != 0;
    }

    private bool IsYInput()
    {
        return _joystick.Vertical != 0;
    }

    private bool IsKnockBackCooledDown()
    {
        return Time.time > _knockBackCooldownTime;
    }
    #endregion
}