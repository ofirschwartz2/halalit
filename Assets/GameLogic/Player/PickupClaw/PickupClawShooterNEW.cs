using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class PickupClawShooterNEW : MonoBehaviour
{
    [SerializeField]
    private float _targetFindingCircleRadius;
    [SerializeField]
    private GameObject _pickupClawPrefab;
    [SerializeField]
    private GameObject _halalit;

    private bool _isClawAlive;
    private GameObject _livingClaw;

    void Start()
    {
        _livingClaw = null;
        _isClawAlive = false;
    }

    void FixedUpdate()
    {
        if (!_isClawAlive)
        {
            var grabbableTarget = TryGetGrabbableTarget();
            if (grabbableTarget != null)
            {
                _livingClaw = InstantiatePickupClaw(grabbableTarget);
                _isClawAlive = true;
            }
        } else 
        {
            if (_livingClaw == null)
            {
                _isClawAlive = false;
            }
        }
    }

    #region Finding Target
    private GameObject TryGetGrabbableTarget() 
    {
        if (!Input.GetMouseButtonDown(0)) 
        {
            return null;
        }

        var targetCircleCenter = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return PickupClawUtils.TryGetClosestGrabbableTarget(targetCircleCenter, _targetFindingCircleRadius);
    }
    #endregion

    #region Claw Instantiation
    private GameObject InstantiatePickupClaw(GameObject target)
    {
        GameObject claw = Instantiate(_pickupClawPrefab, transform.position, Quaternion.identity);
        claw.GetComponent<PickupClawStateMachineNEW>().SetHalalit(_halalit);
        claw.GetComponent<PickupClawStateMachineNEW>().SetTarget(target);
        return claw;
    }
    #endregion

}
