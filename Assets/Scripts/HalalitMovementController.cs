using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    public bool UseConfigFile;
    public float ForceMultiplier;
    public float SpinSpeed;
    public Joystick Joystick;
    public float halalitThrust;
    public float SpeedLimit;
    public float coolDownInterval;
    public float cooldownTime = 0;

    private Rigidbody2D _rigidBody;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        if (UseConfigFile)
        {
            string[] props = { "ForceMultiplier", "SpinSpeed", "_rigidBody.drag", "halalitThrust", "SpeedLimit", "coolDownInterval" };
            Dictionary<string, float> propsFromConfig = ConfigFileReader.GetPropsFromConfig(GetType().Name, props);

            ForceMultiplier = propsFromConfig["ForceMultiplier"];
            SpinSpeed = propsFromConfig["SpinSpeed"];
            _rigidBody.drag = propsFromConfig["_rigidBody.drag"];
            halalitThrust = propsFromConfig["halalitThrust"];
            SpeedLimit = propsFromConfig["SpeedLimit"];
            coolDownInterval = propsFromConfig["coolDownInterval"];
        }
    }
    
    void Update()
    {
        if (CooldownFromCollision())
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
            float joystickAngle = Utils.Vector2ToDegree(Joystick.Horizontal, Joystick.Vertical);
            float rotationZ = transform.rotation.eulerAngles.z;

            float normalizedJoystickAngle = Utils.AngleNormalizationBy360(joystickAngle);
            float normalizedRotationZ = Utils.AngleNormalizationBy360(rotationZ);

            float deltaAngle = normalizedJoystickAngle - normalizedRotationZ;
            float shorterDeltaAngle = Utils.GetShorterSpin(deltaAngle);

            transform.Rotate(new Vector3(0, 0, shorterDeltaAngle) * Time.deltaTime * SpinSpeed);
        }
    }

    private void MoveInRotateDirection()
    {
        if (IsMovementInput() && IsUnderSpeedLimit())
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalForce = direction.x * Math.Abs(Joystick.Horizontal) * ForceMultiplier;
            float verticalForce = direction.y * Math.Abs(Joystick.Vertical) * ForceMultiplier;

            _rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
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

    private bool IsUnderSpeedLimit()
    {
        return Utils.VectorToAbsoluteValue(_rigidBody.velocity) < SpeedLimit;
    }
    #endregion

    #region Knockback

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
            KnockBack(other);
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, otherCollider2D.GetComponent<Rigidbody2D>(), halalitThrust), ForceMode2D.Impulse);
        cooldownTime = Time.time + coolDownInterval;
    }

    private bool CooldownFromCollision()
    {
        return Time.time > cooldownTime;
    }
    
    #endregion
}