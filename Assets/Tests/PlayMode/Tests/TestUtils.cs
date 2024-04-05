using Assets.Enums;
using Assets.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class TestUtils
{
    #region Constants
    // Scenes:
    public const string TEST_SCENE_WITH_TARGET_NAME = "TestingWithTarget";
    public const string TEST_SCENE_WITHOUT_TARGET_NAME = "Testing"; 
    public const string TEST_SCENE_FOR_BOUNCES = "TestingForBounces";

    // Defaults:
    public const int DEFAULT_RADIUS_OF_TARGET_POSITION_AROUND_HALALIT = 5;
    public const ItemRank DEFAULT_ITEM_RANK_1 = ItemRank.COMMON;
    public const ItemRank DEFAULT_ITEM_RANK_2 = ItemRank.RARE;
    public const int DEFAULT_POWER_1 = 1;
    public const int DEFAULT_POWER_2 = 5;
    public const float DEFAULT_CRITICAL_HIT = 1;
    public const float DEFAULT_LUCK = 0;
    public const float DEFAULT_RATE_1 = 2;
    public const float DEFAULT_RATE_2 = 0.5f;
    public const float DEFAULT_WEIGHT = 0;
    public static readonly AttackStats DEFAULT_ATTACK_STATS_1 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_1, DEFAULT_CRITICAL_HIT, DEFAULT_LUCK, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_2 = new(DEFAULT_ITEM_RANK_2, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT, DEFAULT_LUCK, DEFAULT_RATE_2, DEFAULT_WEIGHT);
    public static readonly Vector2 DEFAULT_POSITION_TO_THE_RIGHT = new(5, 0);
    #endregion

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

    #region Scene Setup
    internal static void SetUpShot(AttackName attackName, AttackStats attackStats = null)
    {
        var attackToggle = GetAttackToggle();
        attackToggle.SetNewAttack(attackName, attackStats ?? DEFAULT_ATTACK_STATS_1);
    }

    //TesingWithTarget Scene
    internal static void SetTargetPosition(Vector2 targetPosition)
    {
        TesingWithTargetValidation();

        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[0].transform.position = targetPosition;
    }

    internal static void SetRandomTargetPosition(float radiusOfTargetPositionAroundHalalit = DEFAULT_RADIUS_OF_TARGET_POSITION_AROUND_HALALIT)
    {
        TesingWithTargetValidation();

        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[0].transform.position = Utils.GetRandomVector2OnCircle(radiusOfTargetPositionAroundHalalit);
    }

    internal static void RotateTarget(float degrees) 
    {
        TesingWithTargetValidation();
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[0].transform.rotation = Quaternion.Euler(0, 0, degrees);
    }
    //TesingWithTarget Scene

    #endregion

    #region Scene Getters
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

    //TesingWithTarget Scene
    internal static Vector2 GetTargetPosition()
    {
        TesingWithTargetValidation();

        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return target.transform.position;
    }

    internal static Vector2 GetTargetMovementDirection()
    {
        TesingWithTargetValidation();
        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return target.GetComponent<Rigidbody2D>().velocity.normalized;
    }
    internal static Vector2 GetTargetNearestPositionToHalalit()
    {
        TesingWithTargetValidation();
        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return GetNearestPositionToHalalit(target);
    }

    internal static float GetTargetHealth()
    {
        TesingWithTargetValidation();
        var target = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return target.GetComponent<Health>().GetHealth();
    }
    //TesingWithTarget Scene

    //TestingForBounces Scene
    internal static List<int> GetAllTargetsHealth()
    {
        TesingWithMultipleTargetValidation();
        var targets = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription()).ToList();
        return targets.Select(target => target.GetComponent<Health>().GetHealth()).ToList();
    }
    //TestingForBounces Scene

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
    internal static void TesingWithTargetValidation()
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (target.Length != 1)
        {
            throw new System.Exception("There should be only one target in the scene");
        }
    }

    internal static void TesingWithMultipleTargetValidation() 
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (target.Length <= 1)
        {
            throw new System.Exception("There should be multiple targets in the scene");
        }
    }
    #endregion

    #region Shot Actions
        public static IEnumerator GetDescreteShotPositionHittingTarget()
    {
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;

        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
            yield return lastShotPosition;
        } 
        while (shot != null);
    }

    public static IEnumerator GetConsecutiveShotPositionHitingTarget(int expectedHits, float attackCooldown, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;
        Vector2 newLastShotPosition;
        int hits = 0;
        float nextCooldownTime = 0;

        do
        {
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            newLastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            if (newLastShotPosition == lastShotPosition && CooldownPassed(nextCooldownTime))
            {
                hits++;
                nextCooldownTime = Time.time + attackCooldown;
            }
        }
        while (newLastShotPosition != lastShotPosition || hits != expectedHits);

        yield return lastShotPosition;
    }

    public static IEnumerator GetTimeBetweenProjectedDescreteShot(int expectedShotCount, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        int shotsCount = 0;
        float startTime = Time.time;

        do
        {
            bool isShotProjected = weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            shotsCount += isShotProjected ? 1 : 0;
        }
        while (shotsCount != expectedShotCount);

        yield return Time.time - startTime;
    }

    public static IEnumerator GetTimeBetweenHittingConsecutiveShot(int expectedHits, WeaponAttack weaponAttack, Vector2 attackJoystickTouch)
    {
        int hits = 0;
        float lastTargetHealth = GetTargetHealth();
        float newTargetHealth;
        float lastHitTime = 0;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        Vector2 lastShotPosition;
        Vector2 newLastShotPosition;

        do
        {
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            newLastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            newTargetHealth = GetTargetHealth();
            if (newLastShotPosition == lastShotPosition && newTargetHealth != lastTargetHealth)
            {
                lastTargetHealth = newTargetHealth;
                hits++;

                if (hits != expectedHits)
                {
                    lastHitTime = Time.time;
                }
            }
        }
        while (hits != expectedHits);

        yield return Time.time - lastHitTime;
    }

    private static bool CooldownPassed(float nextCooldownTime)
    {
        return Time.time >= nextCooldownTime;
    }
    #endregion
}