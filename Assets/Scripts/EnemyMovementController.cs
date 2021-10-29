using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public Rigidbody2D _rigidBody;
    public float xSpeed = -0.001f;
    public float ySpeed = -0.003f;
    
    void Update()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x + xSpeed, _rigidBody.velocity.y + ySpeed);
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        //Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }
}
