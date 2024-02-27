using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PickupClawShooter : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private PickupClawMovement _pickupClawMovement;
    [SerializeField]
    private PickupClawRetractor _pickupClawRetractor;
    [SerializeField]
    private PickupClawState _pickupClawState;
    [SerializeField]
    private GameObject _pickupClawTarget;
    [SerializeField]
    private float _targetCircleRadius;

    public void TryShoot()
    {
        if (Input.GetMouseButtonDown(0) && _pickupClawState.Value == PickupClawStateE.IN_HALALIT && ShootPointIsValid())
        {
            _pickupClawState.Value = PickupClawStateE.MOVING_FORWARD;
        }
    }

    private bool ShootPointIsValid()
    {
        return !EventSystem.current.IsPointerOverGameObject();
    }

    public void Shoot()
    {
        Vector3 goal = InitShooting();

        _pickupClawRetractor.SetGoal(goal);
        _pickupClawMovement.Move(goal);
    }

    private Vector3 InitShooting()
    {
        _pickupClawState.Value = PickupClawStateE.MOVING_FORWARD;
        gameObject.transform.parent = null;
        _pickupClawMovement.SetPerfectRotationToHalalit(false);
        var targetCircleCenter = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        List<GameObject> optionalTargets = GetOptionalTargets(targetCircleCenter);
        return GetClosestTarget(optionalTargets, targetCircleCenter);
    }

    private List<GameObject> GetOptionalTargets(Vector3 targetCircleCenter)
    {
        var optionalTargets = new List<GameObject>();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(targetCircleCenter, _targetCircleRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject.CompareTag(Tag.ITEM.GetDescription()))
            {
                optionalTargets.Add(collider.gameObject);
            }
        }

        return optionalTargets;
    }

    private GameObject GetClosestTarget(List<GameObject> optionalTargets, Vector3 targetCircleCenter)
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
}
