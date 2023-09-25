using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class MirrorBallShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;

    void Start()
    {
        _rigidBody.velocity = transform.up * _speed;
    }

    private void GoInMirrorDirection(Collider2D other) 
    {
        var mirrorDirection = Utils.GetDirectionFromCollision(_rigidBody.transform.position, other.transform.position);
        _rigidBody.velocity = mirrorDirection * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription())) 
        {
            GoInMirrorDirection(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription())) 
        {
            Destroy(gameObject);
        }
    }
}