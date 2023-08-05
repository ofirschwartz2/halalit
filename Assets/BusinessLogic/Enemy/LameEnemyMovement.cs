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

        _direction = Utils.GetRandomVector2OnCircle();
    }

    void Update()
    {
        EnemyUtils.MoveInStraightLine(_rigidBody, _direction, _movementAmplitude, _rigidBody.velocity.magnitude, _speedLimit);
    }

    private void Die()
    {
        Destroy(gameObject);
    }
    
    private void GoInAnotherDirection()
    {
        _direction = Utils.GetRandomVector2OnOtherHalfOfCircle(_direction);
        _rigidBody.velocity = new Vector2(0f, 0f);  
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
        Console.WriteLine("Background");
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
