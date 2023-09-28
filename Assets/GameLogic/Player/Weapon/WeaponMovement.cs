using Assets.Utils;
using UnityEngine;

class WeaponMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private float _weaponSpinRadius;

    void Update()
    {
        if (AttackJoystickPressed())
        {
            ChangeWeaponPosition();
        }
    }
    private void ChangeWeaponPosition()
    {
        float attackJoystickAngle = GetAttackJoystickAngle();
        Vector3 weaponPosition = Utils.AngleAndRadiusToPointOnCircle(attackJoystickAngle, _weaponSpinRadius) + _halalit.transform.position;
        Quaternion weaponRotation = Quaternion.AngleAxis(attackJoystickAngle - 90f, Vector3.forward);

        transform.SetPositionAndRotation(weaponPosition, weaponRotation);
    }

    private float GetAttackJoystickAngle()
    {
        Vector2 attackJoystickDirection = new(_attackJoystick.Horizontal, _attackJoystick.Vertical);

        if (_attackJoystick.Vertical < 0)
        {
            return Vector2.Angle(attackJoystickDirection, Vector2.left) + 180f;
        }
            
        return Vector2.Angle(attackJoystickDirection, Vector2.right);
    }

    private bool AttackJoystickPressed()
    {
        return _attackJoystick.Horizontal != 0 || _attackJoystick.Vertical != 0;
    }
}
