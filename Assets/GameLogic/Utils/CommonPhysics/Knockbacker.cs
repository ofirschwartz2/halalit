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
        if (IsGameObjectToKnockback(other.collider))
        {
            KnockbackOther(other.collider);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (IsGameObjectToKnockback(other))
        {
            KnockbackOther(other);
        }
    }

    private bool IsGameObjectToKnockback(Collider2D other)
    {
        if (_knockbackeesDescriptions.Contains(other.gameObject.tag))
        {
            return true;
        }

        return false;
    }

    private void KnockbackOther(Collider2D other)
    {
        Rigidbody2D otherRigidBody = other.GetComponent<Rigidbody2D>();

        Vector2 knockbackDirection = (other.transform.position - transform.position).normalized;
        float knockbackSpeed = GetMyTotalSpeed() * _knockbackMultiplier;
        otherRigidBody.AddForce(knockbackDirection * knockbackSpeed, ForceMode2D.Impulse);
    }

    private float GetMyTotalSpeed()
    {
        return GetMyLinearSpeed() + GetMyAngularSpeed();
    }

    private float GetMyLinearSpeed()
    {
        if (_rigidbody2D.isKinematic)
        {
            return GetComponent<KinematicMovement>().GetSpeed();
        }

        return _rigidbody2D.velocity.magnitude;
    }

    private float GetMyAngularSpeed()
    {
        if (_rigidbody2D.isKinematic)
        {
            return GetComponent<KinematicMovement>().GetRotationSpeed();
        }

        return _rigidbody2D.angularVelocity;
    }
}