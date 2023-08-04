using System;
using System.ComponentModel;
using UnityEngine;
namespace Assets.Utils
{
    static class Utils
    {
        #region Math 
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
            return degree * Mathf.PI / 180;
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

        public static float AngleNormalizationBy1(float angle)
        {
            while (angle < -1)
                angle += 1;

            while (angle > 1)
                angle -= 1;

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

        public static Vector2 GetRandomVector(float minX, float maxX, float minY, float maxY)
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

        public static Vector3 GetDirectionVector(Vector3 from, Vector3 to)
        {
            return new Vector3(to.x - from.x, to.y - from.y, 0).normalized;
        }

        public static float GetDirectionalForce(float direction, float force, float multiplier)
        {
            return direction * force * multiplier;
        }

        public static Vector3 Get180RandomNormalizedVector(Vector3 generalDirection)
        {
            float newX = AngleNormalizationBy1(generalDirection.x + UnityEngine.Random.Range(-1f, 1f));
            float newY = AngleNormalizationBy1(generalDirection.y + UnityEngine.Random.Range(-1f, 1f));

            return new Vector3(newX, newY);
        }
        #endregion

        #region Enum Extentions
        public static string GetDescription(this Enum val)
    {
        DescriptionAttribute[] attributes = (DescriptionAttribute[])val
           .GetType()
           .GetField(val.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);
        return attributes.Length > 0 ? attributes[0].Description : string.Empty;
    }
        #endregion
    }
}
