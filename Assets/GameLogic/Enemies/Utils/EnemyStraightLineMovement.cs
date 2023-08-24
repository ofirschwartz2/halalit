using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

static class EnemyStraightLineMovement
{
    private static float _deltaTimeMultiplier = 300f;
    private static float _speedUpOutOfStraightLine = 5f/10f;
    private static float _slowDownOutOfStraightLine = 9f/10f;
    private static float _isAboutToStopTH = 0.35f; // TODO: should be a function of the force applied? 

    public static void MoveInStraightLine(
        Rigidbody2D rigidbody, 
        Vector2 direction, 
        float amplitude, 
        float movementInterval,
        float movementStartTime)
    {
        switch (GetStageOfStraightLineMovement(movementInterval, movementStartTime)) 
        {
            case StraightLineMovementState.SPEED_UP:
                StraightLineSpeedUp(rigidbody, direction, amplitude);
                break;
            case StraightLineMovementState.SLOW_DOWN:
                StraightLineSlowDown(rigidbody, direction, amplitude);
                break;
            case StraightLineMovementState.DONE:
                break;
        }
    }

    public static void StraightLineSpeedUp(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        rigidbody.AddForce(direction * amplitude * (Time.deltaTime * _deltaTimeMultiplier));
    }

    public static void StraightLineSlowDown(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        if (!IsAboutToStop(rigidbody.velocity.magnitude) || DidSwitchDirection(rigidbody.velocity.normalized, direction))
        {
            rigidbody.AddForce(direction * amplitude * -1 * (Time.deltaTime * _deltaTimeMultiplier));
        }
    }

    private static StraightLineMovementState GetStageOfStraightLineMovement(float movementInterval, float movementStartTime)
    {
        var timePassed = Time.time - movementStartTime;

        if (timePassed < movementInterval * _speedUpOutOfStraightLine)
        {
            return StraightLineMovementState.SPEED_UP;
        }
        else if (timePassed < movementInterval * _slowDownOutOfStraightLine)
        {
            return StraightLineMovementState.SLOW_DOWN;
        }
        else
        {
            return StraightLineMovementState.DONE;
        }
    }

    public static void MoveUnderSpeedLimit(Rigidbody2D rigidbody, Vector2 direction, float movementAmplitude, float speedLimit)
    {
        if (Utils.IsUnderSpeedLimit(rigidbody.velocity, speedLimit))
        {
            rigidbody.AddForce(direction * movementAmplitude * (Time.deltaTime * _deltaTimeMultiplier));
        }
    }

    private static bool IsAboutToStop(float speed)
    {
        // TODO: consider doing this predicate as a function of the force applied.
        return Math.Abs(speed) < _isAboutToStopTH;
    }

    private static bool DidSwitchDirection(Vector2 direction, Vector2 oldDirection)
    {
        return direction.x * oldDirection.x <= 0 || direction.y * oldDirection.y <= 0;
    }
}