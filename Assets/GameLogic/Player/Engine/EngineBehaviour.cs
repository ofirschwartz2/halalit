using Assets.Utils;
using System;
using UnityEngine;

class EngineBehaviour : MonoBehaviour
{
    [SerializeField]
    private Joystick _movementJoystick;

    [SerializeField]
    private float _fireAngle;

    [SerializeField]
    private float _fireForceMultiplier;

    [SerializeField]
    private GameObject _engineFirePrefab;

    void Update()
    {
        SetFireSize();
    }

    private void SetFireSize()
    {
        if (Utils.IsTrueOneOf(2) && MovementJoystickPressed()) 
        {
            var movementForce = Utils.GetLengthOfLine(Math.Abs(_movementJoystick.Horizontal), Math.Abs(_movementJoystick.Vertical)) * _fireForceMultiplier;

            var fire = Instantiate(_engineFirePrefab, transform.position, GetFireAngle());
            fire.GetComponent<Rigidbody2D>().AddForce(-Utils.AddAngleToVector(transform.right, UnityEngine.Random.Range(-_fireAngle, _fireAngle)) * movementForce);
        }
    }

    private Quaternion GetFireAngle()
    {
        var randomAngle = UnityEngine.Random.Range(-_fireAngle, _fireAngle);
        return Utils.GetRotationPlusAngle(transform.rotation, randomAngle);
    }

    private bool MovementJoystickPressed()
    {
        return _movementJoystick.Horizontal != 0 || _movementJoystick.Vertical != 0;
    }
}
