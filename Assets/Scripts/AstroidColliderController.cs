using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class AstroidColliderController : MonoBehaviour
{
    public GameObject Astroid;

    void Start()
    {
        tag = "Astroid";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Astroid.SendMessage("InnerOnTriggerEnter2D", other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Astroid.SendMessage("InnerOnTriggerExit2D", other);
    }

}
