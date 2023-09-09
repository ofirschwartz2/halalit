using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

static class EnemyMovementUtils
{
    // TODO (refactor): shouldn't these need to be configurable? 
    private const float DELTA_TIME_MULTIPLIER = 300f;
    private const float SPEED_UP_OUT_OF_STRAIGHT_LINE = 5f/10f;
    private const float SLOW_DOWN_OUT_OF_STRAIGHT_LINE = 9f/10f;
    private const float IS_ABOUT_TO_STOP_TH = 0.35f;

    #region StraightLineMovement
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
        rigidbody.AddForce((Time.deltaTime * DELTA_TIME_MULTIPLIER) * amplitude * direction);
    }

    public static void StraightLineSlowDown(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        if (!IsAboutToStop(rigidbody.velocity.magnitude) || DidSwitchDirection(rigidbody.velocity.normalized, direction))
        {
            rigidbody.AddForce((Time.deltaTime * DELTA_TIME_MULTIPLIER) * -1 * amplitude * direction);
        }
    }

    private static StraightLineMovementState GetStageOfStraightLineMovement(float movementInterval, float movementStartTime)
    {
        var timePassed = Time.time - movementStartTime;

        if (timePassed < movementInterval * SPEED_UP_OUT_OF_STRAIGHT_LINE)
        {
            return StraightLineMovementState.SPEED_UP;
        }
        else if (timePassed < movementInterval * SLOW_DOWN_OUT_OF_STRAIGHT_LINE)
        {
            return StraightLineMovementState.SLOW_DOWN;
        }
        else
        {
            return StraightLineMovementState.DONE;
        }
    }

    private static bool IsAboutToStop(float speed)
    {
        return Math.Abs(speed) < IS_ABOUT_TO_STOP_TH;
    }

    private static bool DidSwitchDirection(Vector2 direction, Vector2 oldDirection)
    {
        return direction.x * oldDirection.x <= 0 || direction.y * oldDirection.y <= 0;
    }
    #endregion

    #region UnderSpeedLimitMovement
    public static void MoveUnderSpeedLimit(Rigidbody2D rigidbody, Vector2 direction, float movementAmplitude, float speedLimit)
    {
        if (Utils.IsUnderSpeedLimit(rigidbody.velocity, speedLimit))
        {
            rigidbody.AddForce((Time.deltaTime * DELTA_TIME_MULTIPLIER) * movementAmplitude * direction);
        }
    }
    #endregion
}