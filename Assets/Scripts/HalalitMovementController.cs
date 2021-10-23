using System;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    const float VERY_SMALL_NUMBER = 0.05f;

    public float VelocityMultiplier, SlowDownVelocity;
    public Joystick Joystick;

    private Rigidbody2D _rigidBody;
    private bool _slowDown;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _slowDown = true;
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
        float horizontalFinalVelocity;
        float verticalFinalVelocity;

        if (NoMovementInput())
            _slowDown = true;
        else
        {
            _slowDown = false;
            Vector2 direction = DegreeToVector2(transform.rotation.eulerAngles.z);

            horizontalFinalVelocity = direction.x * Math.Abs(Joystick.Horizontal) * VelocityMultiplier;
            verticalFinalVelocity = direction.y * Math.Abs(Joystick.Vertical) * VelocityMultiplier;

            _rigidBody.velocity = new Vector2(horizontalFinalVelocity, verticalFinalVelocity);
        }
    }

    private void SlowingDown()
    {
        if (_slowDown && Math.Abs(_rigidBody.velocity.x) > VERY_SMALL_NUMBER)
        {
            float slowDownXVelocity = SlowDownVelocity;

            if (_rigidBody.velocity.x > 0)
                slowDownXVelocity *= -1;

             _rigidBody.AddForce(new Vector2(slowDownXVelocity, 0));
        }

        if (_slowDown && Math.Abs(_rigidBody.velocity.y) > VERY_SMALL_NUMBER)
        {
            float slowDownYVelocity = SlowDownVelocity;
            
            if (_rigidBody.velocity.y > 0)
                slowDownYVelocity *= -1;

             _rigidBody.AddForce(new Vector2(0, slowDownYVelocity));
        }
    }

    private void Stopping()
    {
        if (ValidStopping())
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

    private bool ValidStopping()
    {
        return _slowDown &&
            Math.Abs(_rigidBody.velocity.x) <= VERY_SMALL_NUMBER &&
            Math.Abs(_rigidBody.velocity.y) <= VERY_SMALL_NUMBER;
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