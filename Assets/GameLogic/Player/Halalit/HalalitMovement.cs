using Assets.Enums;
using Assets.Utils;
using System;
using UnityEngine;

public class HalalitMovement : MonoBehaviour
{
    [SerializeField]
    private bool _useConfigFile;
    [SerializeField]
    private float _forceMultiplier;
    [SerializeField]
    private float _spinSpeed;
    [SerializeField]
    private Joystick _joystick;
    [SerializeField]
    private float _halalitThrust;
    [SerializeField]
    private float _speedLimit;
    [SerializeField]
    private float _knockBackCooldownInterval;
    [SerializeField]
    private Rigidbody2D _rigidBody;
    
    private float _rigidBodyDrag;
    private float _knockBackCooldownTime;

    private void OnHalalitDeath(object initiator, DeathEventArguments arguments)
    {
        Destroy(gameObject);
    }

    void Start()
    {
        if (_useConfigFile)
        {
            ConfigFileReader.LoadMembersFromConfigFile(this);
            _rigidBody.drag = _rigidBodyDrag;
        }
    }

    void Update()
    {
        if (IsKnockBackCooledDown())
        {
            RotateByMovementJoystick();
            MoveInRotateDirection();
        }  
    }

    #region Moving 
    private void RotateByMovementJoystick()
    {
        if (IsMovementInput())
        {
            float joystickAngle = Utils.Vector2ToDegree(_joystick.Horizontal, _joystick.Vertical);
            float rotationZ = transform.rotation.eulerAngles.z;

            float normalizedJoystickAngle = Utils.AngleNormalizationBy360(joystickAngle);
            float normalizedRotationZ = Utils.AngleNormalizationBy360(rotationZ);

            float deltaAngle = normalizedJoystickAngle - normalizedRotationZ;
            float shorterDeltaAngle = Utils.GetNormalizedAngleBy360(deltaAngle);

            transform.Rotate(_spinSpeed * Time.deltaTime * new Vector3(0, 0, shorterDeltaAngle));
        }
    }

    private void MoveInRotateDirection()
    {
        if (IsMovementInput() && Utils.IsUnderSpeedLimit(_rigidBody.velocity, _speedLimit))
        {
            Vector2 direction = Utils.DegreeToVector2(transform.rotation.eulerAngles.z);

            float horizontalForce = Utils.GetDirectionalForce(direction.x, Math.Abs(_joystick.Horizontal), _forceMultiplier);
            float verticalForce = Utils.GetDirectionalForce(direction.y, Math.Abs(_joystick.Vertical), _forceMultiplier);

            _rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
        }
    }
    #endregion

    #region Knockback
    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO (dev): Add knockback colliding with asteroids 
        if (other.gameObject.CompareTag(Tag.ENEMY.GetDescription()))
        {
            KnockBack(other);
        }
    }

    private void KnockBack(Collider2D otherCollider2D)
    {
        Vector2 normalizedDifference = (_rigidBody.transform.position - otherCollider2D.transform.position).normalized;
        
        _rigidBody.AddForce(normalizedDifference * Utils.GetNormalizedSpeed(_rigidBody, otherCollider2D.GetComponent<Rigidbody2D>(), _halalitThrust), ForceMode2D.Impulse);
        _knockBackCooldownTime = Time.time + _knockBackCooldownInterval;
    }
    #endregion

    #region Predicates
    private bool IsMovementInput()
    {
        return IsXInput() || IsYInput();
    }

    private bool IsXInput()
    {
        return _joystick.Horizontal != 0;
    }

    private bool IsYInput()
    {
        return _joystick.Vertical != 0;
    }

    private bool IsKnockBackCooledDown()
    {
        return Time.time > _knockBackCooldownTime;
    }
    #endregion
}