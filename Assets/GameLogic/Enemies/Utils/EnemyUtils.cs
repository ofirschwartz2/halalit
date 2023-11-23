using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

static class EnemyUtils
{
    private const float DELTA_TIME_MULTIPLIER = 300f; // TODO (refactor): should be configurable

    public static Vector2 GetAnotherDirectionFromEdge(Rigidbody2D rigidbody, string edge)
    {
        rigidbody.velocity = new Vector2(0f, 0f);
        Dictionary<string, Vector2> newDirections = new()
        {
            { Tag.TOP_EDGE.GetDescription(), Vector2.down },
            { Tag.RIGHT_EDGE.GetDescription(), Vector2.left },
            { Tag.BOTTOM_EDGE.GetDescription(), Vector2.up },
            { Tag.LEFT_EDGE.GetDescription(), Vector2.right },
        };
        
        return Utils.GetRandomVector2OnHalfOfCircle(newDirections[edge]);
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