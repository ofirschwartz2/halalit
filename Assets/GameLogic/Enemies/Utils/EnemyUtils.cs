using Assets.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

static class EnemyUtils
{
    private const float DELTA_TIME_MULTIPLIER = 300f; // TODO (refactor): should be configurable

    public static Vector2 GetAnotherDirectionFromEdge(Rigidbody2D rigidbody, string edge)
    {
        rigidbody.velocity = new Vector2(0f, 0f);
        var direction = edge switch
        {
            "TopEdge" => Vector2.down,
            "RightEdge" => Vector2.left,
            "BottomEdge" => Vector2.up,
            "LeftEdge" => Vector2.right,
            _ => throw new Exception("Unknown edge: " + edge),
        };
        return Utils.GetRandomVector2OnHalfOfCircle(direction);
    }

    public static void MoveUnderSpeedLimit(Rigidbody2D rigidbody, Vector2 direction, float movementAmplitude, float speedLimit)
    {
        if (Utils.IsUnderSpeedLimit(rigidbody.velocity, speedLimit))
        {
            rigidbody.AddForce((Time.deltaTime * DELTA_TIME_MULTIPLIER) * movementAmplitude * direction);
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
}