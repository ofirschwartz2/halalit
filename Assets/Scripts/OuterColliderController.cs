using System.Collections;
using System.Collections.Generic;
using Assets.Common;
using UnityEngine;

public class OuterColliderController : MonoBehaviour
{
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        other = ParentIfOuterCollider(other);
        transform.parent.gameObject.SendMessage("InnerOnTriggerEnter2D", other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        other = ParentIfOuterCollider(other);
        transform.parent.gameObject.SendMessage("InnerOnTriggerExit2D", other);
    }

    private Collider2D ParentIfOuterCollider(Collider2D other)
    {
        if(other.gameObject.CompareTag("AstroidOuterCollider") || other.gameObject.CompareTag("EnemyOuterCollider"))
            return other.transform.parent.gameObject.GetComponent<Collider2D>();
        return other;
    }

}
