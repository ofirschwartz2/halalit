using Assets.Common;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AstroidtMovementController : MonoBehaviour
{
    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = new Vector2(-2f, -2f);
    }
}