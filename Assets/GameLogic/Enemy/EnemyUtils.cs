using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

static class EnemyUtils
{
    public static void MoveInStraightLine(
        Rigidbody2D rigidbody, 
        Vector2 direction, 
        float amplitude, 
        float movementInterval,
        float movementStartTime)
    {
        switch (GetStageOfStraightLineMovement(movementInterval, movementStartTime)) 
        {
            case StraightLineMovementStage.SPEED_UP:
                StraightLineSpeedUp(rigidbody, direction, amplitude);
                break;
            case StraightLineMovementStage.SLOW_DOWN:
                StraightLineSlowDown(rigidbody, direction, amplitude);
                break;
            case StraightLineMovementStage.DONE:
                break;
        }
    }

    public static void StraightLineSpeedUp(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        rigidbody.AddForce(direction * amplitude * (Time.deltaTime * 300));
    }

    public static void StraightLineSlowDown(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        if (!IsAboutToStop(rigidbody.velocity.magnitude))
        {
            rigidbody.AddForce(direction * amplitude * -1 * (Time.deltaTime * 300));
        }
    }

    private static StraightLineMovementStage GetStageOfStraightLineMovement(float movementInterval, float movementStartTime)
    {
        var timePassed = Time.time - movementStartTime;
        if (timePassed < movementInterval * 5 / 10)
        {
            return StraightLineMovementStage.SPEED_UP;
        }
        else if (timePassed < movementInterval * 9 / 10)
        {
            return StraightLineMovementStage.SLOW_DOWN;
        }
        else
        {
            return StraightLineMovementStage.DONE;
        }
    }

    public static Vector2 GetAnotherDirectionFromEdge(Rigidbody2D rigidbody, string edge)
    {
        Vector2 direction;
        rigidbody.velocity = new Vector2(0f, 0f);
        switch (edge)
        {
            case "TopEdge":
                direction = Vector2.down;
                break;
            case "RightEdge":
                direction = Vector2.left;
                break;
            case "BottomEdge":
                direction = Vector2.up;
                break;
            case "LeftEdge":
                direction = Vector2.right;
                break;
            default:
                throw new Exception("Unknown edge: " + edge);
        }
        return Utils.GetRandomVector2OnHalfOfCircle(direction);
    }

    #region Predicates
    private static bool IsAboutToStop(float speed)
    {
        return Math.Abs(speed) < 0.3f;
    }

    private static bool IsUnderSpeedLimit(float speed, float speedLimit)
    {
        return speed < speedLimit;
    }

    public static bool ShouldKillMe(Collider2D other)
    {
        //TODO: refactor this. should be in a centralizes place.
        return other.gameObject.CompareTag("Shot");
    }

    public static bool ShouldKnockMeBack(Collider2D other)
    {
        //TODO: refactor this. should be in a centralizes place, and get both colliders to decide.
        return other.gameObject.CompareTag("Halalit") || other.gameObject.CompareTag("Astroid") || other.gameObject.CompareTag("Enemy");
    }
    #endregion
}