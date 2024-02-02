using Assets.Utils;
using UnityEngine;

public class MoveAimAttackMove : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _movingInterval;
    [SerializeField]
    private float _rotationSpeed;

    private Vector2 _direction;
    private float _movingTime;
    
    void Start()
    {
        SetDirection();
        SetMoving();
    }

    public void SetDirection()
    {
        _direction = Utils.GetRandomVector2OnCircle();
    }

    public void MovingState()
    {
        if (_rigidBody.constraints != RigidbodyConstraints2D.FreezeRotation)
        {
            RotateTowardsHalalit();
        }
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _direction, _movementAmplitude, _speedLimit);
    }

    public bool DidMovingTimePass()
    {
        return Time.time > _movingTime;
    }

    private void RotateTowardsHalalit() 
    {
        var halalitDirection = Utils.GetHalalitDirection(transform.position);
        var targetRotation = Quaternion.LookRotation(Vector3.forward, halalitDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SetMoving()
    {
        SetMovingTime();
    }

    private void SetMovingTime()
    {
        _movingTime = Time.time + _movingInterval;
    }

    public void SetNewDirection(string edgeTag)
    {
        _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, edgeTag);
    }
}