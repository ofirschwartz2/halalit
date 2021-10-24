using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HalalitCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("hit detected");
    }
    private void OnCollisionEnter2D(Collision2D collider)
    {
        Debug.Log("hitt detected");
    }
    void ProcessCollision(GameObject collider)
    {
        Debug.Log("hittt detected");
    }
}
