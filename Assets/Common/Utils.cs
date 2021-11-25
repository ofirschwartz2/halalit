using System;
using UnityEngine;
namespace Assets.Common
{
    static class Utils
    {
        public static float Vector2ToDegree(float x, float y)
        {
            return Mathf.Atan2(y, x) * Mathf.Rad2Deg;
        }

        public static Vector2 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static Vector2 RadianToVector2(float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static float VectorToAbsoluteValue(Vector2 vector2)
        {
            return GetLengthOfLine(vector2.x, vector2.y);
        }

        public static float AngleNormalizationBy360(float angle)
        {
            if (angle < 0)
                angle += 360;

            return angle;
        }

        public static float GetLengthOfLine(float x, float y)
        {
            return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        }

        public static float GetNormalizedSpeed(Rigidbody2D myRigidBody2D, Rigidbody2D otherRigidBody2D, float thrust)
        {
            return (Utils.VectorToAbsoluteValue(myRigidBody2D.velocity) + Utils.VectorToAbsoluteValue(otherRigidBody2D.velocity)) * thrust;
        }
        public static Vector2 GetRandomVector(int minX, int maxX, int minY, int maxY)
        {
            return new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        }
    }
}
