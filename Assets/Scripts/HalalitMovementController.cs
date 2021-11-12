using System;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    const float COOL_DOWN_INTERVAL = 0.1f;
    public float cooldownTime = 0;
    public float speedLimit = 10;

    public float VelocityMultiplier; // = 10;
    public float SpinSpeed;
    public Joystick Joystick;
    private Rigidbody2D _rigidBody;
    private float _halalitThrust;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _rigidBody.drag = 2;
        _halalitThrust = 5f;
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
            float joystickAngle = Vector2ToDegree(Joystick.Horizontal, Joystick.Vertical);
            float rotationZ = transform.rotation.eulerAngles.z;

            float normalizedJoystickAngle = AngleNormalizationBy360(joystickAngle);
            float normalizedRotationZ = AngleNormalizationBy360(rotationZ);

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
        if (IsMovementInput() && IsUnderSpeedLimit())
        {
            Vector2 direction = DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalVelocity = direction.x * Math.Abs(Joystick.Horizontal) * VelocityMultiplier;
            float verticalVelocity = direction.y * Math.Abs(Joystick.Vertical) * VelocityMultiplier;

            _rigidBody.AddForce(new Vector2(horizontalVelocity, verticalVelocity));
        }
    }

    #endregion

    #region Predicates

    private bool IsMovementInput()
    {
        return !NoXInput() || !NoYInput();
    }

    private bool IsUnderSpeedLimit()
    {
        return VectorToAbsoluteValue(_rigidBody.velocity) < speedLimit;
    }

    private bool CooldownFromCollision()
    {
        return Time.time > cooldownTime;
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
        return VectorToAbsoluteValue(_rigidBody.velocity);
    }

    #endregion

    #region Math Helper Function

    public static float Vector2ToDegree(float x, float y)
    {
        return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
    }

    public static float VectorToAbsoluteValue(Vector2 vector2)
    {
        return (float)Math.Sqrt(Math.Pow(vector2.x, 2) + Math.Pow(vector2.y, 2));
    }

    public static float AngleNormalizationBy360(float angle)
    {
        if (angle < 0)
            angle += 360;

        return angle;
    }

    #endregion

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
            KnockBack(other);
    }
    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * _halalitThrust, ForceMode2D.Impulse);
        cooldownTime = Time.time + COOL_DOWN_INTERVAL;
    }
}