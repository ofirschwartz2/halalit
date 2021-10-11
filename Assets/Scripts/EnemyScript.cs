using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D hitInfo){
        Debug.Log(hitInfo.name); 
        Destroy(gameObject);
    }
}
