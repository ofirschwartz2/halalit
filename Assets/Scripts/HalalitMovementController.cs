using System;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    const float STOP_THRESHOLD = 0.07f;
    const float ALMOST_ZERO = 0.000001f;

    public float VelocityMultiplier; // = 10;
    public float SlowDownVelocity; // = 2;
    public Joystick Joystick;

    private Rigidbody2D _rigidBody;
    private bool _noMovementInput;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _noMovementInput = true;
    }

    void Update()
    {
        RotateByMovementJoystick();
        MoveInRotateDirection();
        SlowingDown();
        Stopping();
    }

    #region Moving 

    private void RotateByMovementJoystick()
    {
        if (!IsNoMovementInput())
        {
            float angle = Vector2ToDegree(Joystick.Horizontal, Joystick.Vertical);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void MoveInRotateDirection()
    {
        if (IsNoMovementInput())
            _noMovementInput = true;
        else
        {
            _noMovementInput = false;
            Vector2 direction = DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalVelocity = direction.x * Math.Abs(Joystick.Horizontal) * VelocityMultiplier;
            float verticalVelocity = direction.y * Math.Abs(Joystick.Vertical) * VelocityMultiplier;

            _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        }
    }

    #endregion

    #region Slowing down

    private void SlowingDown()
    {
        if (ShouldSlowDown())
            _rigidBody.AddForce(getSlowDownVelocity());

    }

    private Vector2 getSlowDownVelocity()
    {
        float slowDownYVelocity = getSlowDownYVelocity();
        float slowDownXVelocity = getSlowDownXVelocity(slowDownYVelocity);

        if (_rigidBody.velocity.x > 0)
            slowDownXVelocity *= -1;

        if (_rigidBody.velocity.y > 0)
            slowDownYVelocity *= -1;

        return new Vector2(slowDownXVelocity, slowDownYVelocity);
    }

    private float getSlowDownYVelocity()
    {
        float numerator = (float)Math.Pow(SlowDownVelocity, 2);
        float denominator = (float)Math.Pow(_rigidBody.velocity.x / _rigidBody.velocity.y, 2) + 1;

        if (denominator == 0)
            denominator = ALMOST_ZERO;

        return numerator / denominator;
    }

    private float getSlowDownXVelocity(float slowDownYVelocity)
    {
        return (float)Math.Sqrt(Math.Abs(Math.Pow(SlowDownVelocity, 2) - Math.Pow(slowDownYVelocity, 2)));
    }

    private void Stopping()
    {
        if (ShouldStop())
            _rigidBody.velocity = Vector2.zero;
    }

    #endregion

    #region Predicates

    private bool IsNoMovementInput()
    {
        return NoXInput() && NoYInput();
    }

    private bool NoXInput()
    {
        return Joystick.Horizontal == 0;
    }

    private bool NoYInput()
    {
        return Joystick.Vertical == 0;
    }

    private bool ShouldStop()
    {
        return _noMovementInput && getAbsoluteSpeed() <= STOP_THRESHOLD;
    }

    private bool ShouldSlowDown()
    {
        return _noMovementInput && getAbsoluteSpeed() > STOP_THRESHOLD;
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

    #endregion
}