using Assets.Enums;
using Assets.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class Knockbackee : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody2D;
    [SerializeField]
    private List<Tag> _knockbackers;
    [SerializeField]
    private float _knockbackMultiplier;

    private float _speedBeforeKnockback;
    private float _angularSpeedBeforeKnockback;
    private List<string> _knockbackersDescriptions;
    private KinematicMovement _kinematicMovement;

    #region Init
    public void Awake()
    {
        _knockbackersDescriptions = _knockbackers.Select(tag => Utils.GetDescription(tag)).ToList();

        if (_rigidbody2D.isKinematic)
        {
            _kinematicMovement = GetComponent<KinematicMovement>();
        }
    }
    #endregion

    #region Accessors / Mutators
    private void FixedUpdate()
    {
        if (_rigidbody2D.isKinematic)
        {
            _speedBeforeKnockback = _kinematicMovement.GetSpeed();
            _angularSpeedBeforeKnockback = _kinematicMovement.GetRotationSpeed();
        }
        else
        {
            _speedBeforeKnockback = _rigidbody2D.velocity.magnitude;
            _angularSpeedBeforeKnockback = _rigidbody2D.angularVelocity;
        }
    }

    public float GetSpeedBeforeKnockback()
    {
        return _speedBeforeKnockback;
    }

    public float GetAngularSpeedBeforeKnockback()
    {
        return _angularSpeedBeforeKnockback;
    }
    #endregion

    #region Apply knockback
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (IsGameObjectToKnockbackMe(other.gameObject))
        {
            KnockbackMe(other.gameObject);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (IsGameObjectToKnockbackMe(other.gameObject))
        {
            KnockbackMe(other.gameObject);
        }
    }

    private void KnockbackMe(GameObject other)
    {
        Vector2 knockbackDirection = CalculateKnockbackDirection(other);
        float knockbackSpeed = (GetLinearSpeed(other) + (GetAngularSpeed(other) * Constants.ANGULAR_SPEED_KNOCKBACK_MULTIPLIER)) * _knockbackMultiplier;
        _rigidbody2D.AddForce(knockbackDirection * knockbackSpeed, ForceMode2D.Impulse);
    }

    private Vector2 CalculateKnockbackDirection(GameObject other)
    {
        var otherDirection = other.GetComponent<Rigidbody2D>().velocity.normalized;
        Vector2 targetToOtherDirection = (transform.position - other.transform.position).normalized;
        var combinedVector = (otherDirection + targetToOtherDirection).normalized;
        return combinedVector;
    }

    #endregion

    #region Speed calculations
    private float GetLinearSpeed(GameObject other)
    {
        Rigidbody2D otherRigidBody = other.GetComponent<Rigidbody2D>();

        if (IsKnockbackee(other) && IsMyKnockbackee(other))
        {
            return other.GetComponent<Knockbackee>().GetSpeedBeforeKnockback();
        }
        else
        {
            return otherRigidBody.isKinematic ? other.GetComponent<KinematicMovement>().GetSpeed() : otherRigidBody.velocity.magnitude;
        }
    }

    private float GetAngularSpeed(GameObject other)
    {
        Rigidbody2D otherRigidBody = other.GetComponent<Rigidbody2D>();

        if (IsKnockbackee(other) && IsMyKnockbackee(other))
        {
            return other.GetComponent<Knockbackee>().GetAngularSpeedBeforeKnockback();
        }
        else
        {
            return otherRigidBody.isKinematic ? other.GetComponent<KinematicMovement>().GetRotationSpeed() : otherRigidBody.angularVelocity;
        }
    }
    #endregion

    #region Predicates
    private bool IsGameObjectToKnockbackMe(GameObject other)
    {
        if (other.tag != null) 
        {
            return IsKnockbackeeBy(other.tag);
        }

        Debug.LogWarning("Missing tag in " + other.name + " game object");
        return false;
    }

    private bool IsKnockbackee(GameObject other)
    {
        return other.GetComponent<Knockbackee>() != null;
    }

    private bool IsMyKnockbackee(GameObject other)
    {
        Knockbackee otherKnockbackee = other.GetComponent<Knockbackee>();

        return otherKnockbackee.IsKnockbackeeBy(gameObject.tag);
    }

    public bool IsKnockbackeeBy(String tag)
    {
        return _knockbackersDescriptions.Contains(tag);
    }
    #endregion
}