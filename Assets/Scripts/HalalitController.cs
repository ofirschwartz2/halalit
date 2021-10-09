using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        rigidBody.AddForce(new Vector2(horizontalInput, verticalInput));
    }
}
