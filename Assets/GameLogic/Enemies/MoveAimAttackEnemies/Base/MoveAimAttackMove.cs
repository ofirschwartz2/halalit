using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class MoveAimAttackMove : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _movingInterval;
    [SerializeField]
    private float _rotationSpeed;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction;
    private float _movingTime;
    
    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetDirection();
        SetMoving();
    }

    public void SetDirection()
    {
        _direction = Utils.GetRandomVector2OnCircle();
    }

    public void MovingState()
    {
        RotateTowardsHalalit();
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

    private void Die()
    {
        Destroy(gameObject);
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: refactor this. should this be in the EventHandler?
        if (EnemyUtils.ShouldKillMe(other))
        {
            Die();
        }
        else if (EnemyUtils.ShouldKnockEnemyBack(LayerMask.LayerToName(gameObject.layer), other))
        {
            EnemyUtils.KnockMeBack(_rigidBody, other);
        }
        else if (Utils.DidHitEdge(other.gameObject.tag))
        {
            SetNewDirection(other.gameObject.tag);
        }
    }
    #endregion
}