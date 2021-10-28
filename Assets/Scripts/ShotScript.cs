using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotScript : MonoBehaviour
{
    public float speed = 10f;
    public Rigidbody2D _rigidbody;
    void Start()
    {
        _rigidbody.velocity = transform.right * speed;
    }
    void OnTriggerEnter2D(Collider2D hitInfo){
        Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }

}
