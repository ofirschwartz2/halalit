using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("TestsPlayMode")]
#endif

public class PickupClawShooter : MonoBehaviour
{
    [SerializeField]
    private float _targetFindingCircleRadius;
    [SerializeField]
    private GameObject _pickupClawPrefab;
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private GameObject[] _joysticks;


    private bool _isClawAlive;
    private GameObject _livingClaw;

    void Start()
    {
        _livingClaw = null;
        _isClawAlive = false;
    }

    void FixedUpdate()
    {
        var isMounseButtonDown = Input.GetMouseButtonDown(0);

        if (isMounseButtonDown)
        {
            var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryShootClaw(mousePosition);
        }
    }

#if UNITY_EDITOR
    internal
#else
    private
#endif
    void TryShootClaw(Vector2 position)
    {
        if (!_isClawAlive)
        {
            var grabbableTarget = TryGetGrabbableTarget(position);
            if (grabbableTarget != null)
            {
                _livingClaw = InstantiatePickupClaw(grabbableTarget);
                _isClawAlive = true;
            }
        }
        else
        {
            if (_livingClaw == null)
            {
                _isClawAlive = false;
            }
        }
    }

    #region Finding Target
    private GameObject TryGetGrabbableTarget(Vector3 targetCircleCenter) 
    {
        if (IsOnJoysticks(targetCircleCenter)) 
        {
            return null;
        }

        return PickupClawUtils.TryGetClosestGrabbableTarget(targetCircleCenter, _targetFindingCircleRadius);
    }
    #endregion

    #region Claw Instantiation
    private GameObject InstantiatePickupClaw(GameObject target)
    {
        GameObject claw = Instantiate(_pickupClawPrefab, transform.position, Quaternion.identity);
        claw.GetComponent<PickupClawStateMachine>().SetHalalit(_halalit);
        claw.GetComponent<PickupClawStateMachine>().SetTarget(target);
        return claw;
    }
    #endregion

    #region Predicates
    private bool IsOnJoysticks(Vector2 targetCircleCenter) 
    {
        foreach (var joystick in _joysticks)
        {
            if (Vector2.Distance(joystick.transform.position, targetCircleCenter) < joystick.transform.localScale.x)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Getters
    internal GameObject GetPickupClawPrefab()
    {
        return _pickupClawPrefab;
    }
    #endregion


}
