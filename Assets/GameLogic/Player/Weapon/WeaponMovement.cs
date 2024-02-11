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
        TryChangeWeaponPosition(_attackJoystick);
    }

    public void TryChangeWeaponPosition(Joystick attackJoystick)
    {
        if (AttackJoystickPressed(attackJoystick))
        {
            ChangeWeaponPosition(attackJoystick);
        }
    }

    private void ChangeWeaponPosition(Joystick attackJoystick)
    {
        float attackJoystickAngle = GetAttackJoystickAngle(attackJoystick);
        Vector3 weaponPosition = Utils.AngleAndRadiusToPointOnCircle(attackJoystickAngle, _weaponSpinRadius) + _halalit.transform.position;
        Quaternion weaponRotation = Quaternion.AngleAxis(attackJoystickAngle - 90f, Vector3.forward);

        transform.SetPositionAndRotation(weaponPosition, weaponRotation);
    }

    private float GetAttackJoystickAngle(Joystick attackJoystick)
    {
        Vector2 attackJoystickDirection = new(attackJoystick.Horizontal, attackJoystick.Vertical);

        if (attackJoystick.Vertical < 0)
        {
            return Vector2.Angle(attackJoystickDirection, Vector2.left) + 180f;
        }
            
        return Vector2.Angle(attackJoystickDirection, Vector2.right);
    }

    private bool AttackJoystickPressed(Joystick attackJoystick)
    {
        return attackJoystick.Horizontal != 0 || attackJoystick.Vertical != 0;
    }
}
