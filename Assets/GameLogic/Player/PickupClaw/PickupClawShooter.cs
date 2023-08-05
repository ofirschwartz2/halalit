using Assets.Enums;
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

        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
