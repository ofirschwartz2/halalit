using UnityEngine;

static class EnemyUtils
{
    public static void MoveInStraightLine(Rigidbody2D rigidbody, Vector2 direction, float amplitude, float speed, float speedLimit)
    {

        if (IsUnderSpeedLimit(speed, speedLimit))
        {
            rigidbody.AddForce(direction * amplitude);
        }
    }

    #region Predicates
    public static bool IsUnderSpeedLimit(float speed, float speedLimit)
    {
        return speed < speedLimit;
    }

    public static bool ShouldKillMe(Collider2D other)
    {
        //TODO: refactor this. should be in a centralizes place.
        return other.gameObject.CompareTag("Shot") || other.gameObject.CompareTag("LazerShot");
    }

    public static bool ShouldKnockMeBack(Collider2D other)
    {
        //TODO: refactor this. should be in a centralizes place, and get both colliders to decide.
        return other.gameObject.CompareTag("Halalit") || other.gameObject.CompareTag("Astroid") || other.gameObject.CompareTag("Enemy");
    }
    #endregion
}