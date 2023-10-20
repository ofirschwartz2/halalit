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
        if (MovementJoystickPressed()) 
        {
            var movementForce = Utils.GetLengthOfLine(Math.Abs(_movementJoystick.Horizontal), Math.Abs(_movementJoystick.Vertical)) * _fireForceMultiplier;

            var fire = Instantiate(_engineFirePrefab, transform.position, GetFireAngle());
            fire.GetComponent<Rigidbody2D>().AddForce(transform.up * movementForce);
        }
    }

    private Quaternion GetFireAngle()
    {
        var randomAngle = UnityEngine.Random.Range(-_fireAngle, _fireAngle);
        return Utils.GetRotationPlusAngle(Quaternion.identity, randomAngle);
    }

    private bool MovementJoystickPressed()
    {
        return _movementJoystick.Horizontal != 0 || _movementJoystick.Vertical != 0;
    }
}
