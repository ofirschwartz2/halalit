using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using UnityEngine;

public class BankShot : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed = 9f;
    [SerializeField]
    private float _lifetime = 4f;
    // Index = bounces so far. The bounce that would reach the end of the list removes the disc.
    [SerializeField]
    private List<float> _harmMultipliersByBounce = new() { 0.5f, 2f, 3f, 4f };

    private AttackBehaviour _attackBehaviour;
    private float _endOfLifeTime;
    private int _bounces;

    // Awake, not Start: the disc can hit in its first physics step, before its Start.
    private void Awake()
    {
        _attackBehaviour = GetComponent<AttackBehaviour>();
        _bounces = 0;
        UpdateHarmMultiplier();
    }

    void Start()
    {
        _endOfLifeTime = Utils.GetEndOfLifeTime(_lifetime);
        _rigidBody.velocity = transform.up * _speed;
    }

    private void FixedUpdate()
    {
        if (Utils.ShouldDie(_endOfLifeTime))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            BounceOffAsteroid(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            BounceOffWall(other.bounds);
        }
    }

    private void BounceOffAsteroid(Collider2D asteroid)
    {
        var bounceDirection = Utils.GetDirectionFromCollision(_rigidBody.position, asteroid.transform.position);
        _rigidBody.velocity = bounceDirection * _speed;
        Bounce();
    }

    private void BounceOffWall(Bounds world)
    {
        Vector2 position = _rigidBody.position;
        Vector2 velocity = _rigidBody.velocity;

        if (IsMovingOutOf(position.x, velocity.x, world.min.x, world.max.x))
        {
            velocity.x = -velocity.x;
        }

        if (IsMovingOutOf(position.y, velocity.y, world.min.y, world.max.y))
        {
            velocity.y = -velocity.y;
        }

        _rigidBody.velocity = velocity;
        Bounce();
    }

    private void Bounce()
    {
        _bounces++;

        if (IsOutOfBounces())
        {
            Destroy(gameObject);
            return;
        }

        UpdateHarmMultiplier();
    }

    private void UpdateHarmMultiplier()
    {
        _attackBehaviour.SetHarmMultiplier(_harmMultipliersByBounce[_bounces]);
    }

    #region Predicates
    private bool IsOutOfBounces()
    {
        return _bounces >= _harmMultipliersByBounce.Count;
    }

    private bool IsMovingOutOf(float position, float speed, float min, float max)
    {
        return (position > max && speed > 0) || (position < min && speed < 0);
    }
    #endregion

#if UNITY_EDITOR
    internal float GetLifeTime()
    {
        return _lifetime;
    }

    internal int GetBounces()
    {
        return _bounces;
    }

    internal void SetBounces(int bounces)
    {
        _bounces = bounces;
        UpdateHarmMultiplier();
    }
#endif
}
