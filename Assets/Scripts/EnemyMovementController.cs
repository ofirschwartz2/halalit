using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementController : MonoBehaviour
{
    public Rigidbody2D _rigidBody;
    void Update()
    {
        _rigidBody.velocity = new Vector2(_rigidBody.velocity.x - 0.001f, _rigidBody.velocity.y - 0.003f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo){
        Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }
}
