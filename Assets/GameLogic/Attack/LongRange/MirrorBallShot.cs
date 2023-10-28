using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class MirrorBallShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _spreadingMultiplier;

    private Vector3 _initialScale;

    void Start()
    {
        _rigidBody.velocity = transform.up * _speed;
        _initialScale = transform.localScale;
    }

    void FixedUpdate()
    {
        var newYScale = transform.localScale.y + _speed * _spreadingMultiplier;
        transform.localScale = new Vector2(transform.localScale.x, newYScale);
    }

    private void GoInMirrorDirection(Collider2D other) 
    {
        transform.rotation = Utils.GetRorationOutwards(other.transform.position, transform.position);
        _rigidBody.velocity = transform.up * _speed;
        transform.localScale = _initialScale;
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