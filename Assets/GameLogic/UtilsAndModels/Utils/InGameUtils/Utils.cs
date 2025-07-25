﻿using Assets.Enums;
using System;
using System.ComponentModel;
using UnityEngine;
using TMPro;

namespace Assets.Utils
{
    public static class Utils
    {
        private static GameObject _internalWorld;
        private static GameObject _externalSafeIsland;

        #region Init
        public static void SetGameObjects(GameObject internalWorld, GameObject externalSafeIsland)
        {
            _internalWorld = internalWorld;
            _externalSafeIsland = externalSafeIsland;
        }
        #endregion

        #region Math 
        public static float Vector2ToDegrees(Vector2 vector)
        {
            return Vector2ToDegrees(vector.x, vector.y);
        }

        public static float Vector2ToDegrees(float x, float y) // TODO: DELETE
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
            return degree * Mathf.Deg2Rad;//* Mathf.PI / 180;
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

        // DELETE. use Vector2.Distance(point1, point2)
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

        public static Vector2 GetRandomVector(float minX, float maxX, float minY, float maxY)
        {
            return new Vector2(SeedlessRandomGenerator.Range(minX, maxX), SeedlessRandomGenerator.Range(minY, maxY));
        }
        
        
        public static float GetNormalizedAngleBy360(float angle)
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
            float newX = AngleNormalizationBy1(generalDirection.x + SeedlessRandomGenerator.Range(-1f, 1f));
            float newY = AngleNormalizationBy1(generalDirection.y + SeedlessRandomGenerator.Range(-1f, 1f));

            return new Vector3(newX, newY);
        }

        public static Vector2 ShiftVectorByOffsetDegree(Vector2 vector, float offsetDegrees)
        {
            float offsetRadians = DegreeToRadian(offsetDegrees);

            float vectorAngle = Vector2ToDegrees(vector.x, vector.y);
            float newVectorAngle = vectorAngle + offsetRadians;

            float newVectorX = vector.magnitude * Mathf.Cos(newVectorAngle);
            float newVectorY = vector.magnitude * Mathf.Sin(newVectorAngle);

            return new(newVectorX, newVectorY);
        }

        public static float Vector2ToRadians(Vector2 direction)
        {
            float radians = Mathf.Atan2(direction.y, direction.x);

            if (radians < 0)
            {
                radians += 2 * Mathf.PI;
            }

            return radians;
        }

        public static float GetRandomAngleAround(float range)
        {
            return SeedlessRandomGenerator.Range(-range, range);
        }

        public static float GetRandomBetween(float bottom, float top)
        {
            return SeedlessRandomGenerator.Range(bottom, top);
        }

        public static float GetEndOfLifeTime(float lifetime)
        {
            return Time.time + lifetime;
        }

        public static float GetPortionPassed(float startTime, float duration)
        {
            return (Time.time - startTime) / (duration);
        }
        #endregion

        #region Vectors

        public static Vector2 GetOppositeVector(Vector2 vector)
        {
            return new Vector2(-vector.x, -vector.y);
        }

        public static Vector2 GetDestinationPosition(Vector2 startPosition, Vector2 rotation, float distance) // TODO: move to Utils
        {
            float angleInRadians = Vector2ToRadians(rotation);

            float x = startPosition.x + Mathf.Cos(angleInRadians) * distance;
            float y = startPosition.y + Mathf.Sin(angleInRadians) * distance;

            return new Vector2(x, y);
        }

        public static Vector2 GetRandomVector2OnCircle(float radius = 1)
        {
            float angle = SeedlessRandomGenerator.Range(0, 2 * Mathf.PI);
            return RadianToVector2(angle) * radius;
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

        public static Vector2 RotateVector2WithVector2(Vector2 vectorFromZero, Vector2 rotationVector)
        {
            float angle = Mathf.Atan2(rotationVector.y, rotationVector.x);

            float newX = vectorFromZero.x * Mathf.Cos(angle) - vectorFromZero.y * Mathf.Sin(angle);
            float newY = vectorFromZero.x * Mathf.Sin(angle) + vectorFromZero.y * Mathf.Cos(angle);

            return new Vector2(newX, newY);
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
        {
            var halalitPosition = GetHalalitPosition();
            return new Vector2(halalitPosition.x, halalitPosition.y) - myPosition;
        }

        public static Transform GetHalalitTransform()
        {
            return GameObject.FindGameObjectWithTag("Halalit").transform;
        }

        public static Vector3 GetHalalitPosition()
        {
            return GetHalalitTransform().position;
        }

        #endregion

        #region Quaternions

        public static Vector2 GetRotationAsVector2(Quaternion rotation)
        {
            // This is because we use sprites that are ritated 'UP' - 90 degrees. If we want we can decide to rotate all sprites 'RIGHT' - 0 degrees.
            var upQuaternion = Quaternion.Euler(0f, 0f, 90f);
            // This is because we use sprites that are ritated 'UP' - 90 degrees. If we want we can decide to rotate all sprites 'RIGHT' - 0 degrees.

            rotation = rotation * upQuaternion;
            float angle = rotation.eulerAngles.z;
            float angleRadians = angle * Mathf.Deg2Rad;
            return RadianToVector2(angleRadians);
        }

        public static Quaternion GetRotationPlusAngle(Quaternion rotation, float angle)
        {
            return rotation * Quaternion.AngleAxis(angle, Vector3.forward);
        }

        public static Quaternion GetRorationOutwards(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            return GetRotationPlusAngle(Quaternion.identity, angle);
        }

        public static float QuaternionToDegrees(Quaternion rotation)
        {
            return Vector2ToDegrees(GetRotationAsVector2(rotation));
        }

        #endregion

        #region BezierCurves
        public static Vector2 GetPointOnBezierCurveOf8(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector2 p5, Vector2 p6, Vector2 p7, Vector2 p8, float progress)
        {

            Vector2 q1 = Vector2.Lerp(p1, p2, progress);
            Vector2 q2 = Vector2.Lerp(p2, p3, progress);
            Vector2 q3 = Vector2.Lerp(p3, p4, progress);

            Vector2 q4 = Vector2.Lerp(p5, p6, progress);
            Vector2 q5 = Vector2.Lerp(p6, p7, progress);
            Vector2 q6 = Vector2.Lerp(p7, p8, progress);

            Vector2 r1 = Vector2.Lerp(q1, q2, progress);
            Vector2 r2 = Vector2.Lerp(q2, q3, progress);

            Vector2 r3 = Vector2.Lerp(q4, q5, progress);
            Vector2 r4 = Vector2.Lerp(q5, q6, progress);

            Vector2 finalPoint = Vector2.Lerp(r1, r2, progress);
            Vector2 finalPoint2 = Vector2.Lerp(r3, r4, progress);

            return Vector2.Lerp(finalPoint, finalPoint2, progress);
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

        #region Predicates
        public static bool DidHitEdge(string tag) 
        {
            return tag == Tag.TOP_EDGE.GetDescription() || tag == Tag.RIGHT_EDGE.GetDescription() || tag == Tag.BOTTOM_EDGE.GetDescription() || tag == Tag.LEFT_EDGE.GetDescription();
        }

        public static bool IsUnderSpeedLimit(Vector2 myVelocity, float speedLimit)
        {
            return myVelocity.magnitude < speedLimit;
        }

        public static bool ShouldDie(float endOfLifeTime)
        {
            return Time.time >= endOfLifeTime;
        }

        public static bool Are2VectorsAlmostEqual(Vector2 a, Vector2 b, float delta = 0.001f)
        {
            return Vector2.Distance(a, b) <= delta;
        }

        public static bool IsCloserClockwise(float degreeFrom, float degreeTo) 
        {
            var a = degreeTo > degreeFrom ? degreeTo - degreeFrom > 180 : degreeFrom - degreeTo < 180;
            return a;
        }

        public static bool IsCenterInsideTheWorld(GameObject gameObject)
        {
            Vector3 position3 = new(gameObject.transform.position.x, gameObject.transform.position.y, 1); // TODO: why our InternalWorldBoxCollider2DBoxCollider2D Z=1?
            Debug.Log($"gameObject: {gameObject.tag}");
            Debug.Log($"position3: {position3}");
            Debug.Log($"internalWorld: {_internalWorld.GetComponent<Collider2D>().bounds}");
            return _internalWorld.GetComponent<Collider2D>().bounds.Contains(position3); 
        }

        public static bool IsCenterInExternalSafeIsland(Vector2 position)
        {
            Vector3 position3 = new(position.x, position.y, 1); // TODO: why our InternalWorldBoxCollider2DBoxCollider2D Z=1?
            Debug.Log($"position3: {position3}");
            Debug.Log($"externalSafeIsland: {_externalSafeIsland.GetComponent<Collider2D>().bounds}");
            return _externalSafeIsland.GetComponent<Collider2D>().bounds.Contains(position3);
        }
        #endregion

        #region Visuals
        public static void ChangeOpacity(Renderer renderer, float opacity) 
        {
            Material mat = renderer.material;
            Color color = mat.color;
            color.a = opacity;
            mat.color = color;
        }
        #endregion

        #region SceneManipulations
        public static Transform FindTransformChild(Transform fatherTransaform, string childName)
        {
            var transformChild = fatherTransaform.Find(childName);
            if (transformChild == null)
            {
                throw new System.Exception($"{childName} not found.");
            }

            return transformChild;
        }

        public static void SetTMProTextOnComponent(Transform parentTransform, string childName, string textValue)
        {
            TMPro.TextMeshProUGUI text = parentTransform.Find(childName).GetComponent<TMPro.TextMeshProUGUI>();
            if (text != null)
            {
                text.text = textValue;
            }
            else
            {
                Debug.LogError($"Text component not found on '{childName}' game object.");
            }
        }
        #endregion
    }
}