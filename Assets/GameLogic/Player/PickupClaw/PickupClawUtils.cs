using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

internal static class PickupClawUtils
{
    internal static GameObject TryGetClosestGrabbableTarget(Vector2 targetCircleCenter, float targetFindingCircleRadius)
    {
        List<GameObject> optionalTargets = GetOptionalGrabbableTargets(targetCircleCenter, targetFindingCircleRadius);
        
        if (optionalTargets.Count == 0)
        {
            return null;
        }

        return GetClosestTarget(optionalTargets, targetCircleCenter);
    }

    private static List<GameObject> GetOptionalGrabbableTargets(Vector3 targetCircleCenter, float targetFindingCircleRadius)
    {
        var optionalTargets = new List<GameObject>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetCircleCenter, targetFindingCircleRadius);
        foreach (Collider2D collider in colliders)
        {
            if (IsCollectable(collider.gameObject))
            {
                optionalTargets.Add(collider.gameObject);
            }
        }

        return optionalTargets;
    }

    private static GameObject GetClosestTarget(List<GameObject> optionalTargets, Vector3 targetCircleCenter)
    {
        GameObject closestTarget = null;
        float minDistance = float.MaxValue;
        foreach (GameObject target in optionalTargets)
        {
            float distance = Vector3.Distance(target.transform.position, targetCircleCenter);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTarget = target;
            }
        }
        return closestTarget;
    }

    private static bool IsCollectable(GameObject target)
    {
        return target.CompareTag(Tag.ITEM.GetDescription()) || target.CompareTag(Tag.VALUABLE.GetDescription());
    }
}
