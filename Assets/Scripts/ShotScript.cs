using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody2D _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.velocity = transform.right * speed;
    }
    private void OnTriggerEnter2D(Collider2D hitInfo){
        Debug.Log("SHOT COLLIDE WITH: " + hitInfo.tag);
        Destroy(gameObject);
    }

}
