using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class EnemyColliderController : MonoBehaviour
{
    public GameObject Enemy;

    void Start()
    {
        tag = "Enemy";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("EnemyColliderController, other.tag:" + other.tag);
        Enemy.SendMessage("InnerOnTriggerEnter2D", other);
    }

}
