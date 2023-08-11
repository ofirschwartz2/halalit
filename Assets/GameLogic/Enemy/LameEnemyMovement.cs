using Assets.Utils;
using System;
using UnityEngine;

public class LameEnemyMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _movementAmplitude;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private Vector2 _direction;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        SetDirection();
    }

    void Update()
    {
        if (isUnderSpeedLimit()) 
        {
            _rigidBody.AddForce(_direction * _movementAmplitude * (Time.deltaTime * 300));
        }
    }

    private void SetDirection() 
    {
        _direction = Utils.GetRandomVector2OnCircle();
    }

    private bool isUnderSpeedLimit()
    {
        return _rigidBody.velocity.magnitude < _speedLimit;
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void SetNewDirection(string edgeTag)
    {
        _direction = EnemyUtils.GetAnotherDirectionFromEdge(_rigidBody, edgeTag);
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        Console.WriteLine("Background");
        //TODO: refactor this. should this be in the EventHandler?
        if (EnemyUtils.ShouldKillMe(other))
        {
            Die();
        }
        else if (EnemyUtils.ShouldKnockMeBack(other))
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