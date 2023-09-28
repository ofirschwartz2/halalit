using Assets.Enums;
using Assets.Utils;
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

    void Start()
    {
        _rotationSpeed = Random.Range(-_maxRotation, _maxRotation);
    }

    public float GetScale()
    {
        return transform.localScale.x;
    }

    new void FixedUpdate()
    {
        base.FixedUpdate();
        _asteroidLifeTime += Time.deltaTime;
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
        if (other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) && _asteroidLifeTime >= _transparencyPeriod)
        {
            AsteroidMovement otherAsteroidMovement = other.gameObject.GetComponent<AsteroidMovement>();
            float originalSpeed = _speed;
            Vector2 originalDirection = _direction;

            SetCollisionVelocity(other.contacts[0].normal, otherAsteroidMovement.GetScale());
            SetRotationByVelocity(originalSpeed, originalDirection);
        }
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
        float originalDirectionAngle = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(originalDirection.x, originalDirection.y));
        float currentDirectionAngle = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(_rigidBody.velocity.x, _rigidBody.velocity.y));
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