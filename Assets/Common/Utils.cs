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

        public static Vector3 AngleAndRadiusToPointOnCircle(float degreeAngle, float radius)
        {
            return new Vector3(radius * Mathf.Cos(DegreeToRadian(degreeAngle)), radius * Mathf.Sin(DegreeToRadian(degreeAngle)));
        }
        
        public static float DegreeToRadian(float degree)
        {
            return degree * Mathf.PI/180;
        }

        public static float GetVectorMagnitude(Vector2 vector2)
        {
            return GetLengthOfLine(vector2.x, vector2.y);
        }

        public static float AngleNormalizationBy360(float angle)
        {
            while (angle < 0)
                angle += 360;

            while (angle > 360)
                angle -= 360;

            return angle;
        }

        public static float GetDistanceBetweenTwoPoints(Vector2 point1, Vector2 point2)
        {
            float deltaX = Math.Abs(point1.x - point2.x);
            float deltaY = Math.Abs(point1.y - point2.y);

            return GetLengthOfLine(deltaX, deltaY);
        }

        public static float GetLengthOfLine(float x, float y)
        {
            return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));
        }

        public static float GetNormalizedSpeed(Rigidbody2D myRigidBody2D, Rigidbody2D otherRigidBody2D, float thrust)
        {
            return (Utils.GetVectorMagnitude(myRigidBody2D.velocity) + Utils.GetVectorMagnitude(otherRigidBody2D.velocity)) * thrust;
        }

        public static Vector2 GetRandomVector(int minX, int maxX, int minY, int maxY)
        {
            return new Vector2(UnityEngine.Random.Range(minX, maxX), UnityEngine.Random.Range(minY, maxY));
        }

        public static float GetShorterSpin(float angle)
        {
            if (angle > 180)
                return angle - 360;
            else if (angle < -180)
                return angle + 360;
            else
                return angle;
        }
    }
}
