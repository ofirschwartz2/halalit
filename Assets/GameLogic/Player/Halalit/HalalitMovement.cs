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
    private float _speedLimit;
    
    void FixedUpdate()
    {
        RotateByMovementJoystick();
        MoveInRotateDirection();
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
    #endregion
}