using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using Assets.Enums;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class SinusEnemyMovementController : MonoBehaviour
{
    public float Velocity, XSpeedLimit, YSpeedLimit;

    private Rigidbody2D _rigidBody;          // Reference to the Rigidbody2D component
    private Vector2 UP = new Vector2(0, 1);
    private Vector2 RIGHT = new Vector2(1, 0);
    private Vector2 LEFT = new Vector2(-1, 0);
    private Vector2 _direction = new Vector2(0, -1);

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>();  // Get the reference to the Rigidbody2D component
        _direction = RIGHT;

        tag = "SinusEnemy";
    }

    void Update()
    {
        YMovement();
        XMovement();

    }
    
    private void XMovement()
    {
        if (XIsOverSpeedLimit())
        {
            XChangeDirection();
            _rigidBody.AddForce(_direction * Velocity);
        }
    }

    private void XChangeDirection()
    {
        _direction = _direction == RIGHT ? LEFT : RIGHT;
    }

    private bool XIsOverSpeedLimit()
    {
        return Math.Abs(_rigidBody.velocity.x) > XSpeedLimit ;
    }

    private void YMovement()
    {
        if (YIsUnderSpeedLimit())
        {
            _rigidBody.AddForce(UP * Velocity);
        }
    }
    private bool YIsUnderSpeedLimit()
    {
        return _rigidBody.velocity.y < YSpeedLimit;
    }
}
