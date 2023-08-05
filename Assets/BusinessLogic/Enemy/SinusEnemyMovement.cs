using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class SinusEnemyMovement : MonoBehaviour 
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _xMovementAmplitude;
    [SerializeField]
    private float _changeXDirectionInterval;
    [SerializeField]
    private float _yMovementAmplitude;
    [SerializeField]
    private float _ySpeedLimit;
    [SerializeField]
    private Rigidbody2D _rigidBody;

    private float _changeXForceDirectionTime;
    private Vector2 _xDirection, _yDirection;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _xDirection = Vector2.right;
        _yDirection = Vector2.up;
        SetChangeXForceDirectionTime(_changeXDirectionInterval / 2);
    }

    void Update()
    {
        YMovement();
        XMovement();
    }

    private void XMovement()
    {
        if (DidXTimePass())
        {
            SetChangeXForceDirectionTime(_changeXDirectionInterval);
            XChangeDirection();
        }
        _rigidBody.AddForce(_xDirection * _xMovementAmplitude);
    }

    private void XChangeDirection()
    {
        _xDirection = _xDirection == Vector2.right ? Vector2.left : Vector2.right;
    }

    private bool DidXTimePass()
    {
        return Time.time > _changeXForceDirectionTime;
    }

    private void YMovement()
    {
        if (YIsUnderSpeedLimit())
        {
            _rigidBody.AddForce(_yDirection * _yMovementAmplitude);
        }
    }

    private bool YIsUnderSpeedLimit()
    {
        return Math.Abs(_rigidBody.velocity.y) < _ySpeedLimit;
    }

    private void SetChangeXForceDirectionTime(float interval)
    {
        _changeXForceDirectionTime = Time.time + interval;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    
    private void GoInAnotherDirection()
    {
        _yDirection = _yDirection == Vector2.up? Vector2.down : Vector2.up;
        _rigidBody.velocity = Vector2.zero;
        SetChangeXForceDirectionTime(_changeXDirectionInterval/2);
    }

    private void KnockMeBack(Collider2D other, float otherThrust = 1f)
    {
        /*
        Vector2 normalizedDifference = (_rigidBody.transform.position - other.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, other.GetComponent<Rigidbody2D>(), EnemyThrust * otherThrust), ForceMode2D.Impulse);
        */
    }

    #region Triggers
    void OnTriggerEnter2D(Collider2D other)
    {
        //TODO: refactor this. should this be in the EventHandler?
        if (EnemyUtils.ShouldKillMe(other))
        {
            Die();
        }
        else if (EnemyUtils.ShouldKnockMeBack(other))
        {
            KnockMeBack(other);
        }
        else if (other.gameObject.CompareTag("Background"))
        {
            GoInAnotherDirection();
        }
    }

    #endregion
}
