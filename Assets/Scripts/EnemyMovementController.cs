using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private float xSpeed;
    private float ySpeed;
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        xSpeed = Random.Range(-0.001f, 0.001f);
        ySpeed = Random.Range(-0.002f, 0.002f);
    }

    void Update()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x + xSpeed, _rigidBody.velocity.y + ySpeed);
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        //Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }
}
