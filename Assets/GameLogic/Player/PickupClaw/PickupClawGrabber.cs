using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

internal class PickupClawGrabber : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _radiusToStartGrabbing;
    [SerializeField]
    private float _grabbingMovementSpeed;
    [SerializeField]
    private float _grabbingRotationTime;

    private float _grabbingRotationTimePassed;

    internal GameObject TryGetItemToGrab()
    {
        return PickupClawUtils.TryGetClosestGrabbableTarget(transform.position, _radiusToStartGrabbing);
    }

    internal void GrabTarget(GameObject target)
    {
        if (target == null)
        {
            throw new Exception("Target is null");
        }

        GrabMoveClaw(target);
        GrabRotateClaw(target);
    }

    private void GrabMoveClaw(GameObject target)
    {
        var direction = target.transform.position - transform.position;
        var velocity = direction.normalized * _grabbingMovementSpeed;
        _rigidBody.velocity = velocity;
    }

    private void GrabRotateClaw(GameObject target)
    {
        _grabbingRotationTimePassed += Time.deltaTime;
        var direction = Utils.GetRorationOutwards(transform.position, target.transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            direction,
            Math.Min(_grabbingRotationTimePassed / _grabbingRotationTime, 1));
    }
}