using Assets.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
[assembly: InternalsVisibleTo("Tests")]
#endif

class WeaponMovement : MonoBehaviour
{
    [SerializeField]
    private GameObject _halalit;
    [SerializeField]
    private Joystick _attackJoystick;
    [SerializeField]
    private float _weaponSpinRadius;

    void FixedUpdate()
    {
        #if UNITY_EDITOR
        if (!SceneManager.GetActiveScene().name.Contains("Testing"))
        #endif
            TryChangeWeaponPosition(new Vector2(_attackJoystick.Horizontal, _attackJoystick.Vertical));
    }

#if UNITY_EDITOR
internal
#else
private
#endif
    void TryChangeWeaponPosition(Vector2 attackJoystickTouch)
    {
        if (AttackJoystickPressed(attackJoystickTouch))
        {
            ChangeWeaponPosition(attackJoystickTouch);
        }
    }

    private void ChangeWeaponPosition(Vector2 attackJoystickTouch)
    {
        float attackJoystickAngle = GetAttackJoystickAngle(attackJoystickTouch);
        Vector3 weaponPosition = Utils.AngleAndRadiusToPointOnCircle(attackJoystickAngle, _weaponSpinRadius) + _halalit.transform.position;
        Quaternion weaponRotation = Quaternion.AngleAxis(attackJoystickAngle - 90f, Vector3.forward);

        transform.SetPositionAndRotation(weaponPosition, weaponRotation);
    }

    private float GetAttackJoystickAngle(Vector2 attackJoystickTouch)
    {
        if (attackJoystickTouch.y < 0)
        {
            return Vector2.Angle(attackJoystickTouch, Vector2.left) + 180f;
        }
            
        return Vector2.Angle(attackJoystickTouch, Vector2.right);
    }

    private bool AttackJoystickPressed(Vector2 attackJoystick)
    {
        return attackJoystick.x != 0 || attackJoystick.y != 0;
    }
}
