using Assets.Enums;
using Assets.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Knockbacker : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody2D;
    [SerializeField]
    private List<Tag> _knockbackees;
    [SerializeField]
    private float _knockbackMultiplier;

    private List<string> _knockbackeesDescriptions;

    public void Start()
    {
        _knockbackeesDescriptions = _knockbackees.Select(tag => Utils.GetDescription(tag)).ToList();
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        if (IsGameObjectToKnockback(other))
        {
            KnockBackOther(other);
        }
    }

    private bool IsGameObjectToKnockback(Collision2D other)
    {
        if (_knockbackeesDescriptions.Contains(other.gameObject.tag))
        {
            return true;
        }

        return false;
    }

    private void KnockBackOther(Collision2D other)
    {
        Rigidbody2D otherRigidBody = other.collider.GetComponent<Rigidbody2D>();

        Vector2 knockbackDirection = (other.collider.transform.position -_rigidbody2D.transform.position).normalized;
        float knockBackSpeed = GetMyTotalSpeed() * _knockbackMultiplier;
        otherRigidBody.AddForce(knockbackDirection * knockBackSpeed, ForceMode2D.Impulse);
    }

    private float GetMyTotalSpeed()
    {
        return GetMyLinearSpeed() + GetMyAngularSpeed();
    }

    private float GetMyLinearSpeed()
    {
        if (gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            return GetComponent<AsteroidMovement>().GetSpeed() * Constants.ASTEROID_KNOCKBACK_SPEED_NORMALIZER;
        }

        return _rigidbody2D.velocity.magnitude;
    }

    private float GetMyAngularSpeed()
    {
        if (gameObject.CompareTag(Tag.ASTEROID.GetDescription()))
        {
            return GetComponent<AsteroidMovement>().GetRotationSpeed();
        }

        return _rigidbody2D.angularVelocity;
    }
}

