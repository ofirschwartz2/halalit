using Assets.Enums;
using Assets.Utils;
using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    [SerializeField]
    private float _velocityMagnitude;
    [SerializeField]
    private float _maxRotation;
    [SerializeField]
    private float _collisionSpeedMultiplier;
    [SerializeField]
    private float _transparencyPeriod;

    private float _constantRotation;
    private float _time;
    private Vector2 _direction;

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
        }

        _rigidBody.useFullKinematicContacts = true;
        SetRandomRotationByScale();
        SetVelocity();
    }

    private void SetVelocity()
    {
        _rigidBody.velocity = _direction * _velocityMagnitude;
        //_rigidBody.MovePosition(_rigidBody.position + _direction * _velocityMagnitude * Time.fixedDeltaTime);
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

    public float GetScale()
    {
        return transform.localScale.x;
    }

    void FixedUpdate()
    {
        //_rigidBody.MovePosition(_rigidBody.position + _direction * _velocityMagnitude);
        //_rigidBody.MoveRotation(_rigidBody.rotation + _constantRotation);
        transform.Rotate(0, 0, _constantRotation * Time.deltaTime);
        _direction = _rigidBody.velocity.normalized;
        _time += Time.deltaTime; // TODO: can be removed
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
        if (other.gameObject.CompareTag(Tag.ASTEROID.GetDescription()) && Time.time >= _transparencyPeriod)
        {
            AsteroidMovement otherAsteroidMovement = other.gameObject.GetComponent<AsteroidMovement>();
            Vector2 originalVelocity = _rigidBody.velocity;
            
            SetCollisionVelocity(other.contacts[0].normal, otherAsteroidMovement.GetScale());
            SetRotationByVelocity(originalVelocity);
        }
    }

    private void SetCollisionVelocity(Vector2 contactPointNormal, float otherAsteroidScale)
    {
        float relationalScale = otherAsteroidScale / transform.localScale.x;
        float finalVelocityMultiplier = relationalScale * _collisionSpeedMultiplier;

        _rigidBody.velocity += finalVelocityMultiplier * contactPointNormal;
    }

    private void SetRotationByVelocity(Vector2 originalVelocity)
    {
        float rotationDirectionalMultiplier = GetRotationDirectionalMultiplier(originalVelocity);

        _constantRotation += rotationDirectionalMultiplier * _rigidBody.velocity.magnitude / originalVelocity.magnitude;
    }

    private float GetRotationDirectionalMultiplier(Vector2 originalVelocity)
    {
        float originalDirection = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(originalVelocity.x, originalVelocity.y));
        float currentDirection = Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(_rigidBody.velocity.x, _rigidBody.velocity.y));
        float rotationDirectionalMultiplier;

        if (currentDirection <= originalDirection)
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