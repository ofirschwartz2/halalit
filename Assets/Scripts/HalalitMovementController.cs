using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalalitMovementController : MonoBehaviour
{
    
    public float XVelocityLimit, YVelocityLimit, VelocityMultiplier, SlowDownVelocity;

    private Rigidbody2D _rigidBody;
    private bool _movementInAllDirections;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _movementInAllDirections = true;
    }

    void Update()
    {
        if (_movementInAllDirections)
            MovementInAllDirectionsCycle();
    }

    private void MovementInAllDirectionsCycle()
    {
        Debug.Log("velocity = " + _rigidBody.velocity + "X = " + Input.GetAxis("Horizontal") + " Y = " + Input.GetAxis("Vertical"));

        MovementInAllDirectionsHandler();
        SlowingDownHandler();
    }

    private void MovementInAllDirectionsHandler()
    {
        float horizontalInput = 0;
        float verticalInput = 0;

        if (ValidPositiveXVelocity() || ValidNegativeXVelocity())
        {
            horizontalInput = Input.GetAxis("Horizontal") * VelocityMultiplier;
        }

        if (ValidPositiveYVelocity() || ValidNegativeYVelocity())
        {
            verticalInput = Input.GetAxis("Vertical") * VelocityMultiplier;
        }

        _rigidBody.AddForce(new Vector2(horizontalInput, verticalInput));
    }

    private void SlowingDownHandler()
    {
        if (NoXMovement())
        {
            float slowDownXVelocity = SlowDownVelocity;

            if (_rigidBody.velocity.x > 0)
                slowDownXVelocity *= -1;

             _rigidBody.AddForce(new Vector2(slowDownXVelocity, 0));
        }

        if (NoYMovement())
        {
            float slowDownYVelocity = SlowDownVelocity;

            if (_rigidBody.velocity.y > 0)
                slowDownYVelocity *= -1;

             _rigidBody.AddForce(new Vector2(0, slowDownYVelocity));
        }
    }

    #region predicates

    private bool ValidPositiveXVelocity()
    {
        return _rigidBody.velocity.x <= XVelocityLimit && Input.GetAxis("Horizontal") > 0;
    }

    private bool ValidNegativeXVelocity()
    {
        return _rigidBody.velocity.x > XVelocityLimit * -1 && Input.GetAxis("Horizontal") < 0;
    }

    private bool ValidPositiveYVelocity()
    {
        return _rigidBody.velocity.y <= YVelocityLimit && Input.GetAxis("Vertical") > 0;
    }

    private bool ValidNegativeYVelocity()
    {
        return _rigidBody.velocity.y > YVelocityLimit * -1 && Input.GetAxis("Vertical") < 0;
    }

    private bool NoXMovement()
    {
        return Input.GetAxis("Horizontal") == 0;
    }

    private bool NoYMovement()
    {
        return Input.GetAxis("Vertical") == 0;
    }

    #endregion
}
