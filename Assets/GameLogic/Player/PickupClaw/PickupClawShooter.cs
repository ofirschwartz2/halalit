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
    private float _targetCircleRadius;

    public GameObject TryMoveForward()
    {
        if (
            Input.GetMouseButtonDown(0) && 
            _pickupClawState.Value == PickupClawStateE.IN_HALALIT)
        {
            var targetPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var targetGameObject = TryGetTargetGameObject(targetPoint);

            if (targetGameObject != null) 
            {
                _pickupClawState.Value = PickupClawStateE.MOVING_FORWARD;
                return targetGameObject;
            }
        }
        return null;
    }

    /*
    private bool ShootPointIsValid()
    {
        return !EventSystem.current.IsPointerOverGameObject();
    }
    */

    public void ShootToStaticPosition()
    {
        Vector3 goal = InitShooting();

        _pickupClawRetractor.SetGoal(goal);
        _pickupClawMovement.Move(goal);
    }

    public void InitShooting()
    {
        _pickupClawState.Value = PickupClawStateE.MOVING_FORWARD;
        gameObject.transform.parent = null;
        _pickupClawMovement.SetPerfectRotationToHalalit(false);

        //return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public GameObject TryGetTargetGameObject(Vector2 targetCircleCenter) 
    {
        List<GameObject> optionalTargets = TryGetOptionalTargets(targetCircleCenter);
        return optionalTargets.Count > 0 ? Utils.GetClosestGameObject(optionalTargets, targetCircleCenter) : null;
    }

    private List<GameObject> TryGetOptionalTargets(Vector2 targetCircleCenter)
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
}
