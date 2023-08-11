using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

static class EnemyUtils
{
    private static float _deltaTimeMultiplier = 300f;
    private static float _speedUpOutOfStraightLine = 5f/10f;
    private static float _slowDownOutOfStraightLine = 9f/10f;
    private static float _isAboutToStopTH = 0.35f; // TODO: should be a function of the force applied? 
    private static float _knockbackMultiplier = 300f;

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
        rigidbody.AddForce(direction * amplitude * (Time.deltaTime * _deltaTimeMultiplier));
    }

    public static void StraightLineSlowDown(Rigidbody2D rigidbody, Vector2 direction, float amplitude)
    {
        if (!IsAboutToStop(rigidbody.velocity.magnitude))
        {
            rigidbody.AddForce(direction * amplitude * -1 * (Time.deltaTime * _deltaTimeMultiplier));
        }
    }

    private static StraightLineMovementStage GetStageOfStraightLineMovement(float movementInterval, float movementStartTime)
    {
        var timePassed = Time.time - movementStartTime;

        if (timePassed < movementInterval * _speedUpOutOfStraightLine)
        {
            return StraightLineMovementStage.SPEED_UP;
        }
        else if (timePassed < movementInterval * _slowDownOutOfStraightLine)
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

    public static void KnockMeBack(Rigidbody2D myRigidbody, Collider2D other)
    {
        var knockBackDirection = Utils.GetDirectionFromCollision(myRigidbody.transform.position, other.transform.position);
        myRigidbody.AddForce(knockBackDirection * _knockbackMultiplier);
    }

    #region Predicates
    private static bool IsAboutToStop(float speed)
    {
        return Math.Abs(speed) < _isAboutToStopTH;
    }

    public static bool ShouldKillMe(Collider2D other)
    {
        return other.gameObject.CompareTag("Shot");
    }

    public static bool ShouldKnockMeBack(Collider2D other)
    {
        return other.gameObject.CompareTag("Halalit") || other.gameObject.CompareTag("Astroid") || other.gameObject.CompareTag("Enemy");
    }
    #endregion
}