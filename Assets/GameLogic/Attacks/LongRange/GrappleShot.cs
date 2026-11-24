using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class GrappleShot : MonoBehaviour
{
    [SerializeField]
    private float _shipPullImpulse = 12f;
    [SerializeField]
    private float _enemyPullImpulse = 10f;

    private Rigidbody2D _halalitRigidBody;
    private bool _didPull;

    private void Awake()
    {
        _halalitRigidBody = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription()).GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_didPull)
        {
            return;
        }

        if (IsAsteroid(other.gameObject))
        {
            Vector2 asteroidPosition = other.transform.position;
            Pull(_halalitRigidBody, asteroidPosition, _shipPullImpulse);
            _didPull = true;
        }
        else if (IsEnemy(other.gameObject))
        {
            Pull(other.attachedRigidbody, _halalitRigidBody.position, _enemyPullImpulse);
            _didPull = true;
        }
    }

    private void Pull(Rigidbody2D body, Vector2 target, float impulse)
    {
        body.AddForce((target - body.position).normalized * impulse, ForceMode2D.Impulse);
    }

    #region Predicates
    private bool IsAsteroid(GameObject other)
    {
        return other.CompareTag(Tag.ASTEROID.GetDescription());
    }

    private bool IsEnemy(GameObject other)
    {
        return other.CompareTag(Tag.ENEMY.GetDescription());
    }
    #endregion

#if UNITY_EDITOR
    internal float GetShipPullImpulse()
    {
        return _shipPullImpulse;
    }

    internal float GetEnemyPullImpulse()
    {
        return _enemyPullImpulse;
    }
#endif
}
