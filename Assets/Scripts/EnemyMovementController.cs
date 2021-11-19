using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    private Rigidbody2D _rigidBody;
    private float _xSpeed;
    private float _ySpeed;
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody2D>(); 
        _xSpeed = Random.Range(-0.001f, 0.001f);
        _ySpeed = Random.Range(-0.002f, 0.002f);
    }

    void Update()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x + _xSpeed, _rigidBody.velocity.y + _ySpeed);
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        //Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }
}
