using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _collisionSpeedMultiplier;
    [SerializeField]
    private float _transparencyPeriod;

    private float _constantRotation;
    private float _asteroidLifeTime;
    private Vector2 _direction;

    void Start()
    {
        _rigidBody.useFullKinematicContacts = true;
        SetRandomRotationByScale();
    }

    private void SetRandomRotationByScale()
    {
        _constantRotation = Random.Range(-_maxRotation / transform.localScale.x, _maxRotation / transform.localScale.x);
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public float GetScale()
    {
        return transform.localScale.x;
    }

    public float GetRotationSpeed()
    {
        return Mathf.Abs(_constantRotation);
    }

    void Update()
    {
        _rigidBody.MovePosition(_rigidBody.position + _direction * _speed);
        _rigidBody.MoveRotation(_rigidBody.rotation + _constantRotation);
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

        _constantRotation += rotationDirectionalMultiplier * _speed / originalSpeed;
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