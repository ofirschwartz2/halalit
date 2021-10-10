using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalalitController : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    private float XVelocityLimit = 10;
    private float YVelocityLimit = 10;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = 0;
        float verticalInput = 0;
        Debug.Log("velocity= " + rigidBody.velocity + "X= " + Input.GetAxis("Horizontal") + "Y= " + Input.GetAxis("Vertical"));
        if( !((rigidBody.velocity.x >= XVelocityLimit && Input.GetAxis("Horizontal") > 0) || (rigidBody.velocity.x <= (-1) * XVelocityLimit && Input.GetAxis("Horizontal") < 0)) ) {
            horizontalInput = Input.GetAxis("Horizontal");
        } 
        if( !((rigidBody.velocity.y >= YVelocityLimit && Input.GetAxis("Vertical") > 0) || (rigidBody.velocity.y <= (-1) * YVelocityLimit && Input.GetAxis("Vertical") < 0)) ) {
            verticalInput = Input.GetAxis("Vertical");
        }
        rigidBody.AddForce(new Vector2(horizontalInput, verticalInput));
    }
}
