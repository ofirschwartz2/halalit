using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private float xSpeed;
    private float ySpeed;
    private float enemyThrust;

    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        xSpeed = Random.Range(-0.001f, 0.001f);
        ySpeed = Random.Range(-0.002f, 0.002f);
        enemyThrust = 5f;
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
        _rigidBody.AddForce(normalizedDifference * enemyThrust, ForceMode2D.Impulse);
    }
}
