using UnityEngine;
using System;
using Assets.Utils;

class EngineFireSize : MonoBehaviour
{
    [SerializeField]
    private Joystick _movementJoystick;

    void Update()
    {
        SetFireSize();
    }

    private void SetFireSize()
    {
        float movementForce = MovementJoystickPressed() ? Utils.GetLengthOfLine(Math.Abs(_movementJoystick.Horizontal), Math.Abs(_movementJoystick.Vertical)) : 0;
        float newLocalPositionX = -movementForce / 2;

        transform.localScale = new Vector3(movementForce, transform.localScale.y);
        transform.localPosition = new Vector2(newLocalPositionX, transform.localPosition.y);
    }

    private bool MovementJoystickPressed()
    {
        return _movementJoystick.Horizontal != 0 || _movementJoystick.Vertical != 0;
    }
}
