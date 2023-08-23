using System;
using System.ComponentModel;
using UnityEngine;
using Assets.Enums;

namespace Assets.Utils
{
    static class Utils
    {

        public static float GetRandomAngleAround(float range)
        {
            return UnityEngine.Random.Range(-range, range);
        }

        public static Quaternion GetRotation(Quaternion rotation, float angle)
        {
            return rotation * Quaternion.AngleAxis(angle, Vector3.forward);
        }

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

        public static float GetVectorMagnitude(Vector2 vector2) // we can use Vector2.magnitude
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

        public static float GetLengthOfLine(float x, float y) // we can use Vector2.magnitude
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

        public static bool IsTrueIn50Precent()
        {
            return UnityEngine.Random.Range(0, 2) == 0;
        }

        #endregion

        #region Vectors
        public static Vector2 GetRandomVector2OnCircle()
        {
            float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
            return RadianToVector2(angle);
        }

        public static Vector2 GetRandomVector2OnHalfOfCircle(Vector2 halfCircleDirection)
        {
            var randomVector = GetRandomVector2OnCircle();
            if (Vector2.Dot(randomVector, halfCircleDirection) < 0)
                randomVector *= -1;
            return randomVector;
        }

        public static Vector2 AddAngleToVector(Vector2 vector, float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            float currentAngle = Mathf.Atan2(vector.y, vector.x);
            float newAngle = currentAngle + angleInRadians;
            return RadianToVector2(newAngle);
        }
        public static Vector2 NormalizeVector2(Vector2 vector)
        {
            var magnitude = (float)Math.Sqrt(Math.Pow(vector.x, 2) + Math.Pow(vector.y, 2));
            return new Vector2(vector.x / magnitude, vector.y / magnitude);
        }

        public static Vector2 GetVelocityInDirection(Vector2 velocity, Vector2 direction)
        {
            return Vector2.Dot(velocity, direction) * direction;
        }

        public static Vector2 GetDirectionFromCollision(Vector2 myPosition, Vector2 colliderPosition) 
        {
            var direction = myPosition - colliderPosition;
            direction.Normalize();
            return direction;
        }

        public static Vector2 GetHalalitDirection(Vector2 myPosition)
        {;
            var halalitPosition = GetHalalitPosition();
            return new Vector2(halalitPosition.x, halalitPosition.y) - myPosition;
        }

        public static Vector2 GetHalalitPosition()
        {
            var halalit = GameObject.FindGameObjectWithTag("Halalit");
            return halalit.transform.position;
        }

        public static Vector2 GetRotationAsVector2(Quaternion rotation)
        {
            rotation = rotation * Quaternion.Euler(0f, 0f, 90f); //TODO: talk w Amir
            float angle = rotation.eulerAngles.z;
            float angleRadians = angle * Mathf.Deg2Rad;
            return RadianToVector2(angleRadians);
        }
        #endregion

        public static Quaternion GetRorationOutwards(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        #region Enum Extentions
        public static string GetDescription(this Enum val)
        {
            DescriptionAttribute[] attributes = (DescriptionAttribute[])val
               .GetType()
               .GetField(val.ToString())
               .GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        public static Direction GetRandomDirection()
        {
            return (Direction)UnityEngine.Random.Range(0, Enum.GetValues(typeof(Direction)).Length);
        }
        #endregion

        #region Predicates
        public static bool DidHitEdge(string tag) 
        {
            return tag == "TopEdge" || tag == "RightEdge" || tag == "BottomEdge" || tag == "LeftEdge";
        }

        public static bool IsUnderSpeedLimit(Vector2 myVelocity, float speedLimit)
        {
            return myVelocity.magnitude < speedLimit;
        }
        #endregion
    }
}
