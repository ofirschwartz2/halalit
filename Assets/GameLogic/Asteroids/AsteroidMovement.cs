using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class AsteroidMovement : KinematicMovement
{
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _collisionSpeedMultiplier;
    [SerializeField]
    private float _transparencyPeriod;

    private float _asteroidLifeTime;
    private string _siblingId;

    void Start()
    {
        _rotationSpeed = gameObject.GetComponent<AsteroidSharedBehavior>()._seedfulRandomGenerator.Range(-_maxRotation, _maxRotation);
        if (_siblingId == null)
        {
            _siblingId = Guid.NewGuid().ToString();
        }
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();
        _asteroidLifeTime += Time.deltaTime;

        SpeedLimiter.LimitSpeed(_rigidBody);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(Tag.INTERNAL_WORLD.GetDescription()))
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) &&
            other.gameObject.GetComponent<AsteroidMovement>().GetSiblingId() != _siblingId &&
            _asteroidLifeTime >= _transparencyPeriod
            )
        {
            AsteroidMovement otherAsteroidMovement = other.gameObject.GetComponent<AsteroidMovement>();
            float originalSpeed = _speed;
            Vector2 originalDirection = _direction;

            SetCollisionVelocity(other.contacts[0].normal, otherAsteroidMovement.GetScale());
            SetRotationByVelocity(originalSpeed, originalDirection);
        }
    }

    public float GetScale()
    {
        return transform.localScale.x;
    }

    public void SetSiblingId(string siblingId)
    {
        _siblingId = siblingId;
    }

    private string GetSiblingId()
    {
        return _siblingId;
    }

    private void SetCollisionVelocity(Vector2 contactPointNormal, float otherAsteroidScale)
    {
        float relationalScale = otherAsteroidScale / transform.localScale.x;

        _speed *= relationalScale * _collisionSpeedMultiplier;
        _direction = (_speed * contactPointNormal).normalized;
    }

    private void SetRotationByVelocity(float originalSpeed, Vector2 originalDirection)
    {
        float rotationDirectionalMultiplier = GetRotationDirectionalMultiplier(originalDirection);

        _rotationSpeed += rotationDirectionalMultiplier * _speed / originalSpeed;
    }

    private float GetRotationDirectionalMultiplier(Vector2 originalDirection)
    {
        float originalDirectionAngle = Utils.AngleNormalizationBy360(Utils.Vector2ToDegrees(originalDirection.x, originalDirection.y));
        float currentDirectionAngle = Utils.AngleNormalizationBy360(Utils.Vector2ToDegrees(_rigidBody.velocity.x, _rigidBody.velocity.y));
        float rotationDirectionalMultiplier;

        if (currentDirectionAngle <= originalDirectionAngle)
        {
            rotationDirectionalMultiplier = 1;
        }
        else
        {
            rotationDirectionalMultiplier = -1;
        }

        return rotationDirectionalMultiplier;
    }
}