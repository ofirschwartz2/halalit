using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

static class EnemyUtils
{
    private const float DELTA_TIME_MULTIPLIER = 300f;
    private const float KNOCKBACK_MULTIPLIER = 300f;

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
        myRigidbody.AddForce(knockBackDirection * KNOCKBACK_MULTIPLIER);
    }

    public static void MoveUnderSpeedLimit(Rigidbody2D rigidbody, Vector2 direction, float movementAmplitude, float speedLimit)
    {
        if (Utils.IsUnderSpeedLimit(rigidbody.velocity, speedLimit))
        {
            rigidbody.AddForce(direction * movementAmplitude * (Time.deltaTime * DELTA_TIME_MULTIPLIER));
        }
    }

    public static List<Vector2> GetEvenPositionsAroundCircle(Transform transform, int numberOfPositions, float radius)
    {
        //radius = 0.9f; // TODO: BUG - why is this 0.9 and not 0.5?
        var angle = 360 / numberOfPositions;
        var shootingStartPositions = new List<Vector2>();
        for (var i = 0; i < numberOfPositions; i++)
        {
            shootingStartPositions.Add(transform.position + Utils.AngleAndRadiusToPointOnCircle(angle * i, radius));
        }
        return shootingStartPositions;
    }

    #region Predicates
    public static bool ShouldKillMe(Collider2D other)
    {
        return other.gameObject.CompareTag("Shot");
    }

    public static bool ShouldKnockEnemyBack(string myLayer, Collider2D other)
    {
        return 
            myLayer == "Enemies" &&
            (other.gameObject.CompareTag("Halalit") || 
            other.gameObject.CompareTag("Asteroid") || 
            other.gameObject.CompareTag("Enemy") || 
            other.gameObject.CompareTag("KnockbackShot"));
    }

    #endregion
}