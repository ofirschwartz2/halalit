using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class BallShot : MonoBehaviour 
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;

    void Start()
    {
        _rigidBody.velocity = transform.up * _speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()) || other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) || other.gameObject.CompareTag(Tag.HALALIT.GetDescription()))
            Destroy(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
            Destroy(gameObject);
    }
}