using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private float xSpeed;
    private float ySpeed;
    private float _enemyThrust;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        xSpeed = Random.Range(-0.5f, 0.5f);
        ySpeed = Random.Range(-0.5f, 0.5f);
        _enemyThrust = 0.5f;
    }

    void Update()
    {
        _rigidBody.velocity = new Vector2(xSpeed, ySpeed);
        tag = "Enemy";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            Destroy(gameObject);
        else if (other.gameObject.CompareTag("Halalit"))
            KnockBack(other);
        else if (other.gameObject.CompareTag("Background"))
        {
            Debug.Log("Background HIT");
            GoInAnotherDirection();
        }
    }

    private void GoInAnotherDirection()
    {
            Debug.Log(_rigidBody.transform.position.x + " " + _rigidBody.transform.position.y);

            if (_rigidBody.transform.position.x > 65f)
            {
                xSpeed = Random.Range(-0.005f, 0f);
            } 
            else if (_rigidBody.transform.position.x < -65f)
            {
                xSpeed = Random.Range(0f, 0.005f);
            }
            if (_rigidBody.transform.position.y > 35f)
            {
                ySpeed = Random.Range(-0.005f, 0f);
            } 
            else if (_rigidBody.transform.position.y < -35f)
            {
                ySpeed = Random.Range(0f, 0.005f);
            }
            _rigidBody.velocity = new Vector2(0f, 0f);        
    }
    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * NormalizedSpeed(otherCollider2D), ForceMode2D.Impulse);
    }

    private float NormalizedSpeed(Collider2D otherCollider2D)
    {
        return (Utils.VectorToAbsoluteValue(otherCollider2D.GetComponent<Rigidbody2D>().velocity) + Utils.VectorToAbsoluteValue(_rigidBody.velocity)) * _enemyThrust;
    }
}
