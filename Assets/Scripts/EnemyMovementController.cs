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
        xSpeed = Random.Range(-0.005f, 0.005f);
        ySpeed = Random.Range(-0.005f, 0.005f);
        _enemyThrust = 0.5f;
    }

    void Update()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x + xSpeed, _rigidBody.velocity.y + ySpeed);
        tag = "Enemy";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shot"))
            Destroy(gameObject);
        else if (other.gameObject.CompareTag("Halalit"))
            KnockBack(other);
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        _rigidBody.AddForce(normalizedDifference * normalizedSpeed(otherCollider2D), ForceMode2D.Impulse);
    }

    private float normalizedSpeed(Collider2D otherCollider2D)
    {
        return (Utils.VectorToAbsoluteValue(otherCollider2D.GetComponent<Rigidbody2D>().velocity) + Utils.VectorToAbsoluteValue(_rigidBody.velocity)) * _enemyThrust;
    }
}
