using Assets.Enums;
using UnityEngine;

public class PickupClawRetractorNEW : MonoBehaviour
{

    [SerializeField]
    private float _speedWithTarget;
    [SerializeField]
    private float _speedWithoutTarget;

    private float _speed;

    internal void ReturnToHalalit() 
    {
        
    }

    internal void SetSpeed(bool withTarget) 
    {
        _speed = _speedWithTarget;
    }
}
