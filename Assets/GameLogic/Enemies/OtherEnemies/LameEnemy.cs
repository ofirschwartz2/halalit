using Assets.Utils;
using System;
using UnityEngine;

public class LameEnemy : MonoBehaviour
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
        EnemyUtils.MoveUnderSpeedLimit(_rigidBody, _direction, _movementAmplitude, _speedLimit);
    }

    private void SetDirection() 
    {
        _direction = Utils.GetRandomVector2OnCircle();
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