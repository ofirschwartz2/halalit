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
        Enemy.SendMessage("InnerOnTriggerEnter2D", other);
    }

}
