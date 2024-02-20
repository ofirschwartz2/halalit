using Assets.Enums;
using Assets.Utils;
using UnityEngine;


internal static class TestUtils
{
    #region Random Seeds
    internal static int SetRandomSeed(int seed = 0)
    {
        if (seed == 0)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        return seed;
    }
    #endregion

    #region SceneSetUp
    internal static void SetUpShot(AttackName attackName)
    {
        var attackToggle = GetAttackToggle();
        attackToggle.SetNewAttack(attackName, new AttackStats(ItemRank.COMMON, 1, 1, 1, 1, 1));
    }

    internal static void SetRandomTargetPosition() 
    {
        ScheneWithTargetValidation();

        var radiusOfTargetPositionAroundHalalit = 5;

        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[0].transform.position = Utils.GetRandomVector2OnCircle(radiusOfTargetPositionAroundHalalit);
    }
    #endregion

    #region SceneGetters
    internal static WeaponAttack GetWeaponAttack()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponAttack>();
    }

    internal static AttackToggle GetAttackToggle()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<AttackToggle>();
    }

    internal static WeaponMovement GetWeaponMovement()
    {
        var weapon = GameObject.FindGameObjectWithTag(Tag.WEAPON.GetDescription());
        return weapon.GetComponent<WeaponMovement>();
    }

    internal static BoxCollider2D GetInternalWorldBoxCollider2D()
    {
        return 
            GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription())
                .GetComponent<BoxCollider2D>();
    }

    internal static Vector2 GetTargetPosition()
    {
        ScheneWithTargetValidation();

        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return target.transform.position;
    }

    internal static Vector2 GetTargetNearestPositionToHalalit() 
    {
        ScheneWithTargetValidation();
        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return GetNearestPositionToHalalit(target);
    }

    internal static Vector2 GetNearestPositionToHalalit(GameObject gameObject)
    {
        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var bounds = gameObject.GetComponent<Collider2D>().bounds;
        return bounds.ClosestPoint(halalit.transform.position);
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

    internal static Vector2 GetTouchOverAttackTriggetTowardsPosition(Vector2 position, float attackJoystickEdge)
    {
        var direction = position.normalized;
        return direction * (attackJoystickEdge + 0.1f);
    }
    #endregion

    #region Predicates
    internal static bool IsSomewhereOnInternalWorldEdges(Vector2 position) 
    {
        Vector3 position3 = new Vector3(position.x, position.y, 1); // TODO: why our InternalWorldBoxCollider2DBoxCollider2D Z=1?

        var boxCollider = GetInternalWorldBoxCollider2D();
        var edgeThreshold = 0.05f;
        var bounds = boxCollider.bounds;

        var largerBounds = new Bounds(bounds.center, bounds.size * (1 + edgeThreshold));
        var smallerBounds = new Bounds(bounds.center, bounds.size * (1 - edgeThreshold));

        if (largerBounds.Contains(position3) && !smallerBounds.Contains(position3))
        {
            return true;
        }
        return false;

    }
    #endregion

    #region Validations
    internal static void ScheneWithTargetValidation()
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (target.Length != 1)
        {
            throw new System.Exception("There should be only one target in the scene");
        }
    }
    #endregion
}