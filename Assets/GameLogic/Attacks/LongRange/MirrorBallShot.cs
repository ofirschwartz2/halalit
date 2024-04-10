using Assets.Enums;
using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("PlayModeTests")]
#endif

public class MirrorBallShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private int _maxBounces;

    private int _bounces;
    void Start()
    {
        _bounces = 0;
        _rigidBody.velocity = transform.up * _speed;
    }

    private void GoInMirrorDirection(Collider2D other) 
    {
        var mirrorDirection = Utils.GetDirectionFromCollision(_rigidBody.transform.position, other.transform.position);
        _rigidBody.velocity = mirrorDirection * _speed;
        _bounces++;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription())) 
        {
            if (_bounces == _maxBounces)
            {
                Destroy(gameObject);
            }
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

#if UNITY_EDITOR
    internal int GetMaxBounces()
    {
        return _maxBounces;
    }
#endif
}