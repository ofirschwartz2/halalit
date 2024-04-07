using Assets.Utils;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

class HalalitMovement : MonoBehaviour
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
        #if UNITY_EDITOR
        if (!SceneManager.GetActiveScene().name.Contains("Testing"))
        #endif
            TryMove(_joystick.Horizontal, _joystick.Vertical, Time.deltaTime);
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
        if (Utils.IsUnderSpeedLimit(_rigidBody.velocity, _speedLimit))
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalForce = Utils.GetDirectionalForce(direction.x, Math.Abs(joystickHorizontal), _forceMultiplier);
            float verticalForce = Utils.GetDirectionalForce(direction.y, Math.Abs(joystickVertical), _forceMultiplier);

            _rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
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