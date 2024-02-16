using Assets.Enums;
using Assets.Utils;
using UnityEngine;


internal static class TestUtils
{

    #region SceneSetUp
    internal static void SetUpBallShot()
    {
        var weaponAttack = GetWeaponAttack();

        weaponAttack._currentAttack =
            new KeyValuePair<AttackName, AttackStats>(
                AttackName.BALL_SHOT, 
                new AttackStats(ItemRank.COMMON, 1, 1, 1, 1, 1));
    }
    #endregion

    #region SceneGetters
    internal static WeaponAttack GetWeaponAttack()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponAttack>();
    }

    internal static WeaponMovement GetWeaponMovement()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponMovement>();
    }
    #endregion

    #region Joystick Touchs
    internal static Vector2 GetRandomTouch()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    internal static Vector2 GetRandomTouchUnderAttackTrigger(WeaponAttack weaponAttack) 
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        } while (randomTouch.magnitude >= weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }

    internal static Vector2 GetRandomTouchOverAttackTrigger(float attackJoystickEdge)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        } while (randomTouch.magnitude < attackJoystickEdge);

        return randomTouch;
    }
    #endregion
}