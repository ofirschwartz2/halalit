using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class OuterColliderController : MonoBehaviour
{
    public GameObject Parent;

    void Start()
    {
        tag = Parent.tag;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Parent.SendMessage("InnerOnTriggerEnter2D", other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Parent.SendMessage("InnerOnTriggerExit2D", other);
    }

}
