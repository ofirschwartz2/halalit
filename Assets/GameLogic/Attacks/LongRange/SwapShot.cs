using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class SwapShot : MonoBehaviour
{
    private Rigidbody2D _halalitRigidBody;
    private bool _didSwap;

    private void Awake()
    {
        _halalitRigidBody = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription()).GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_didSwap)
        {
            return;
        }

        if (IsEnemy(other.gameObject))
        {
            Swap(other.attachedRigidbody);
            _didSwap = true;
        }
    }

    private void Swap(Rigidbody2D enemyRigidBody)
    {
        Vector2 halalitPosition = _halalitRigidBody.position;
        _halalitRigidBody.position = enemyRigidBody.position;
        enemyRigidBody.position = halalitPosition;
    }

    #region Predicates
    private bool IsEnemy(GameObject other)
    {
        return other.CompareTag(Tag.ENEMY.GetDescription());
    }
    #endregion
}
