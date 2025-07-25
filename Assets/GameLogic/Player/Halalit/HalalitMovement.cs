using Assets.Utils;
using Assets.Models;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

public class HalalitMovement : MonoBehaviour, ISpeedForceController
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

    private Vector2 _currentForce;

    public float GetSpeedLimit() => _speedLimit;
    public void SetSpeedLimit(float value) 
    { 
        _speedLimit = value;
        Debug.Log($"HalalitMovement: Speed limit set to {value}. Current velocity: {_rigidBody.velocity.magnitude}");
    }

    public float GetForceMultiplier() => _forceMultiplier;
    public void SetForceMultiplier(float value) => _forceMultiplier = value;

    public Vector2 GetCurrentForce() => _currentForce;

    void FixedUpdate()
    {
        TryMove(_joystick.Horizontal, _joystick.Vertical, Time.deltaTime);
        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    #region Moving 
#if UNITY_EDITOR
    internal
#else
    private
#endif
    void TryMove(float joystickHorizontal, float joystickVertical, float deltaTime)
    {
        if (IsMovementInput(joystickHorizontal, joystickVertical))
        {
            RotateByMovementJoystick(joystickHorizontal, joystickVertical, deltaTime);
            MoveInRotationDirection(joystickHorizontal, joystickVertical);
        }
        else
        {
            _currentForce = Vector2.zero;
        }
    }

    private void RotateByMovementJoystick(float joystickHorizontal, float joystickVertical, float deltaTime)
    {
        float joystickAngle = Utils.Vector2ToDegrees(joystickHorizontal, joystickVertical);
        float rotationZ = transform.rotation.eulerAngles.z;

        float normalizedJoystickAngle = Utils.AngleNormalizationBy360(joystickAngle);
        float normalizedRotationZ = Utils.AngleNormalizationBy360(rotationZ);

        float deltaAngle = normalizedJoystickAngle - normalizedRotationZ;
        float shorterDeltaAngle = Utils.GetNormalizedAngleBy360(deltaAngle);

        transform.Rotate(_rotationSpeed * deltaTime * new Vector3(0, 0, shorterDeltaAngle));
    }

    private void MoveInRotationDirection(float joystickHorizontal, float joystickVertical)
    {
        // Allow a small buffer over the speed limit to ensure we can reach it
        if (_rigidBody.velocity.magnitude <= _speedLimit * 1.1f)
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalForce = Utils.GetDirectionalForce(direction.x, Math.Abs(joystickHorizontal), _forceMultiplier);
            float verticalForce = Utils.GetDirectionalForce(direction.y, Math.Abs(joystickVertical), _forceMultiplier);
            _currentForce = new Vector2(horizontalForce, verticalForce);

            _rigidBody.AddForce(_currentForce);
            
            // Log current speed for debugging
            if (_rigidBody.velocity.magnitude > 0)
            {
                Debug.Log($"HalalitMovement: Current speed: {_rigidBody.velocity.magnitude}, Limit: {_speedLimit}");
            }
        }
    }
    #endregion

    #region Predicates
    private bool IsMovementInput(float joystickHorizontal, float joystickVertical)
    {
        return IsXInput(joystickHorizontal) || IsYInput(joystickVertical);
    }

    private bool IsXInput(float joystickHorizontal)
    {
        return joystickHorizontal != 0;
    }

    private bool IsYInput(float joystickVertical)
    {
        return joystickVertical != 0;
    }
    #endregion
}