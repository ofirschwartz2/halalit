using Assets.Enums;
using UnityEngine;

public class PickupClawRetractor : MonoBehaviour
{
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private PickupClawMovement _pickupClawMovement;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private PickupClawState _pickupClawState;

    private Vector3 _goal;

    void Start()
    {
        SetUnreachableGoal();
    }

    private void SetUnreachableGoal()
    {
        Vector3.ClampMagnitude(_goal, int.MaxValue);
    }

    public void TryRetract()
    {
        if (ShouldRetract())
        {
            _pickupClawState.Value = PickupClawStateE.MOVING_BACKWARD;
        }
    }

    public void Retract()
    {
        if (_pickupClawMovement.ReachGoal(_halalit.transform.position))
        {
            _pickupClawState.Value = PickupClawStateE.IN_HALALIT;
        }
        else
        {
            _pickupClawMovement.Move(_halalit.transform.position);
        }
    }

    public void FinalizeRetraction()
    {
        gameObject.transform.SetParent(_halalit.transform); 
        _rigidBody.velocity = Vector2.zero;
        transform.position = new Vector3(_halalit.transform.position.x, _halalit.transform.position.y, 1);
    }

    public void SetGoal(Vector3 goal)
    {
        _goal = goal;
    }

    private bool ShouldRetract()
    {
        return _pickupClawState.Value == PickupClawStateE.MOVING_FORWARD && (_pickupClawMovement.ReachGoal(_goal) || _pickupClawMovement.AtFullRopeLength());
    }
}
