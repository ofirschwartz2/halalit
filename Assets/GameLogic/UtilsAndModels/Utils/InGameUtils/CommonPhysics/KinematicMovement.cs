using UnityEngine;

public abstract class KinematicMovement : MonoBehaviour
{
    [SerializeField]
    protected Rigidbody2D _rigidBody;
    [SerializeField]
    protected float _speed;
    [SerializeField]
    protected Vector2 _direction;
    [SerializeField]
    protected float _rotationSpeed;

    public void FixedUpdate()
    {
        _rigidBody.MovePosition(_rigidBody.position + _speed * Time.deltaTime * _direction);
        _rigidBody.MoveRotation(_rigidBody.rotation + _rotationSpeed * Time.deltaTime);
    }

    public float GetSpeed()
    {
        return _speed;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public float GetRotationSpeed()
    {
        return Mathf.Abs(_rotationSpeed);
    }

    public void SetSpeed(float speed)
    {
        _speed = speed;
    }

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
    }

    public void SetRotationSpeed(float rotationSpeed)
    {
        _rotationSpeed = rotationSpeed;
    }
}