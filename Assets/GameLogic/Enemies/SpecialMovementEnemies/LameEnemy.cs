using Assets.Utils;
using UnityEngine;

public class LameEnemy : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;

    private Vector2 _direction;

    void Start()
    {
        SetDirection();
    }

    void FixedUpdate()
    {
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _direction, _movementAmplitude, _speedLimit);
    }

    private void SetDirection() 
    {
        _direction = Utils.GetRandomVector2OnCircle();
    }

    private void SetNewDirection(string edgeTag)
    {
        _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, edgeTag);
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        if (Utils.DidHitEdge(other.gameObject.tag))
        {
            SetNewDirection(other.gameObject.tag);
        }
    }
    #endregion
}