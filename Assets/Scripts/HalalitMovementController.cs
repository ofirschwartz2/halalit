using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    public bool UseConfigFile;
    public float VelocityMultiplier;
    public float SpinSpeed;
    public Joystick Joystick;

    private Rigidbody2D _rigidBody;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();

        if (UseConfigFile)
        {
            string[] props = { "VelocityMultiplier", "SpinSpeed", "_rigidBody.drag" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            VelocityMultiplier = propsFromConfig["VelocityMultiplier"];
            SpinSpeed = propsFromConfig["SpinSpeed"];
            _rigidBody.drag = propsFromConfig["_rigidBody.drag"];
        }
    }

    void Update()
    {
        RotateByMovementJoystick();
        MoveInRotateDirection();
    }

    #region Moving 

    private void RotateByMovementJoystick()
    {
        if (IsMovementInput())
        {
            float joystickAngle = Utils.Vector2ToDegree(Joystick.Horizontal, Joystick.Vertical);
            float rotationZ = transform.rotation.eulerAngles.z;

            float normalizedJoystickAngle = Utils.AngleNormalizationBy360(joystickAngle);
            float normalizedRotationZ = Utils.AngleNormalizationBy360(rotationZ);

            float deltaAngle = normalizedJoystickAngle - normalizedRotationZ;
            float shorterDeltaAngle = GetShorterSpin(deltaAngle);

            transform.Rotate(new Vector3(0, 0, shorterDeltaAngle) * Time.deltaTime * SpinSpeed);
        }
    }

    private float GetShorterSpin(float angle)
    {
        if (angle > 180)
            return angle - 360;
        else if (angle < -180)
            return angle + 360;
        else
            return angle;
    }

    private void MoveInRotateDirection()
    {
        if (IsMovementInput())
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalVelocity = direction.x * Math.Abs(Joystick.Horizontal) * VelocityMultiplier;
            float verticalVelocity = direction.y * Math.Abs(Joystick.Vertical) * VelocityMultiplier;

            _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        }
    }

    #endregion

    #region Predicates

    private bool IsMovementInput()
    {
        return !NoXInput() || !NoYInput();
    }

    private bool NoXInput()
    {
        return Joystick.Horizontal == 0;
    }

    private bool NoYInput()
    {
        return Joystick.Vertical == 0;
    }

    #endregion

    #region Calculators

    private float getAbsoluteSpeed()
    {
        return Utils.VectorToAbsoluteValue(_rigidBody.velocity);
    }

    #endregion
}