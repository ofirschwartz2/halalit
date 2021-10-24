using System;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    const float STOP_THRESHOLD = 0.05f;

    public float VelocityMultiplier; // = 10;
    public float SlowDownVelocity; // = 2;
    public Joystick Joystick;

    private Rigidbody2D _rigidBody;
    private bool _shouldSlowDown;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _shouldSlowDown = true;
    }

    void Update()
    {
        RotateByMovementJoystick();
        MoveInRotateDirection();
        SlowingDown();
        Stopping();
    }

    private void RotateByMovementJoystick()
    {
        if (!NoMovementInput())
        {
            float angle = Vector2ToDegree(Joystick.Horizontal, Joystick.Vertical);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    private void MoveInRotateDirection()
    {
        if (NoMovementInput())
            _shouldSlowDown = true;
        else
        {
            _shouldSlowDown = false;
            Vector2 direction = DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalVelocity = direction.x * Math.Abs(Joystick.Horizontal) * VelocityMultiplier;
            float verticalVelocity = direction.y * Math.Abs(Joystick.Vertical) * VelocityMultiplier;

            _rigidBody.velocity = new Vector2(horizontalVelocity, verticalVelocity);
        }
    }

    private void SlowingDown()
    {
        if (ShouldSlowDownInDirection(_rigidBody.velocity.x))
        {
            float slowDownXVelocity = SlowDownVelocity;

            if (_rigidBody.velocity.x > 0)
                slowDownXVelocity *= -1;

            _rigidBody.AddForce(new Vector2(slowDownXVelocity, 0));
        }

        if (ShouldSlowDownInDirection(_rigidBody.velocity.y))
        {
            float slowDownYVelocity = SlowDownVelocity;

            if (_rigidBody.velocity.y > 0)
                slowDownYVelocity *= -1;

            _rigidBody.AddForce(new Vector2(0, slowDownYVelocity));
        }
    }

    private void Stopping()
    {
        if (ShouldStop())
            _rigidBody.velocity = Vector2.zero;
    }

    #region predicates

    private bool NoMovementInput()
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
        return _shouldSlowDown &&
            Math.Abs(_rigidBody.velocity.x) <= STOP_THRESHOLD &&
            Math.Abs(_rigidBody.velocity.y) <= STOP_THRESHOLD;
    }

    private bool ShouldSlowDownInDirection(float velocity)
    {
        return _shouldSlowDown && Math.Abs(velocity) > STOP_THRESHOLD;
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

    #endregion
}