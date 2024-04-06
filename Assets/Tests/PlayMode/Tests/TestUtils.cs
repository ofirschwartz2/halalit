using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal static class TestUtils
{
    #region Constants
    // Scenes:
    public const string TEST_SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    public const string TEST_SCENE_WITHOUT_TARGET_NAME = "Testing"; 
    public const string TEST_SCENE_FOR_BOUNCES = "TestingForBounces";
    public const string TEST_SCENE_WITH_MANY_ENEMIES_FROM_RIGHT_NAME = "TestingWithManyEnemiesFromRight";
    public const string TEST_SCENE_WITH_ASTEROID_NAME = "TestingWithAsteroid";
    public const string TEST_SCENE_WITH_MANY_ASTEROIDS_FROM_RIGHT_NAME = "TestingWithManyAsteroidsFromRight";
    public const string TEST_SCENE_WITH_VALUABLES_NAME = "TestingWithValuables";

    // Defaults:
    public const int ENEMIES_SEEDED_NUMBERS_LIST_DEFAULT_LENGTH = 5;
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
            seed = RandomGenerator.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);
        return seed;
    }
    #endregion

    #region Scene SetUp

    internal static void DestroyAllGameObjects() 
    {
        var gameObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (var gameObject in gameObjects)
        {
            Object.Destroy(gameObject);
        }
    }

    internal static void SetUpShot(AttackName attackName, AttackStats attackStats = null)
    {
        var attackToggle = GetAttackToggle();
        attackToggle.SetNewAttack(attackName, attackStats ?? DEFAULT_ATTACK_STATS_1);
    }

    //TesingWithTarget Scene
    internal static void SetTargetPosition(Vector2 targetPosition)
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[0].transform.position = targetPosition;
    }

    internal static void SetRandomEnemyPosition(float radiusOfEnemyPositionAroundHalalit = 5)
    {
        TesingWithOneEnemyValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription()), 
            radiusOfEnemyPositionAroundHalalit);
    }

    internal static void SetRandomAsteroidPosition(float radiusOfAsteroidPositionAroundHalalit = 5)
    {
        TesingWithOneAsteroidValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription()),
            radiusOfAsteroidPositionAroundHalalit);
    }

    internal static void SetRandomItemPosition(float radiusOfItemPositionAroundHalalit = 5)
    {
        TesingWithOneItemValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription()), 
            radiusOfItemPositionAroundHalalit);
    }

    internal static void SetItemPosition(Vector2 position) 
    {
        TesingWithOneItemValidation();

        SetGameObjectPosition(
            GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription()),
            position);
    }

    private static void SetRandomGameObjectPosition(GameObject gameObject, float radiusOfTargetPositionAroundHalalit)
    {
        gameObject.transform.position = Utils.GetRandomVector2OnCircle(radiusOfTargetPositionAroundHalalit);
    }

    private static void SetGameObjectPosition(GameObject gameObject, Vector2 position) 
    {
        gameObject.transform.position = position;
    }

    internal static void RotaeEnemy(float degrees) 
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        enemy[0].transform.rotation = Quaternion.Euler(0, 0, degrees);
    }

    internal static void SetEnemiesSeededNumbers(int listLength = ENEMIES_SEEDED_NUMBERS_LIST_DEFAULT_LENGTH) 
    {
        var enemies = GetEnemies();
        foreach(var enemy in enemies)
        {
            var randomSeededNumbers = new List<float>();
            for (int i = 0; i < listLength; i++)
            {
                randomSeededNumbers.Add(RandomGenerator.RangeZeroToOne(true));
            }
            enemy.GetComponent<RandomSeededNumbers>().SetRandomSeededNumbers(randomSeededNumbers);
        }
    }

    internal static void SetEnemiesHealth(int health)
    {
        var enemies = GetEnemies();
        foreach (var enemy in enemies)
        {
            enemy.GetComponent<Health>().SetHealth(health);
        }
    }

    internal static void SetAsteroidsHealth(int health) 
    {
        var asteroids = GetAsteroids();
        foreach (var asteroid in asteroids)
        {
            asteroid.GetComponent<Health>().SetHealth(health);
        }
    }

    #endregion

    #region SceneGetters

    internal static int GetScore() 
    {
        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var score = halalit.GetComponent<Score>();
        return score.GetScore();
    }

    internal static ValuableName GetValuableName(GameObject valuable) 
    {
        return valuable.GetComponent<Valuable>().GetValuableName();
    }

    internal static List<KeyValuePair<ValuableName, int>> GetValuableValues() 
    {
        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var score = halalit.GetComponent<Score>();
        return score.GetValuableValues();
    }

    internal static HalalitMovement GetHalalitMovement() 
    {
        GameObject halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        return halalit.GetComponent<HalalitMovement>();
    }

    internal static ItemDropper GetItemDropper() 
    {
        var itemsFactory = GameObject.FindGameObjectWithTag(Tag.ITEMS_FACTORY.GetDescription());
        return itemsFactory.GetComponent<ItemDropper>();
    }

    internal static GameObject GetAttackJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.ATTACK_JOYSTICK.GetDescription());
    }

    internal static GameObject GetMovementJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.MOVEMENT_JOYSTICK.GetDescription());
    }

    internal static GameObject GetShot()
    {
        return GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
    }

    internal static GameObject GetPickupClaw()
    {
        return GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
    }

    internal static GameObject GetItem()
    {
        return GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription());
    }

    internal static GameObject[] GetItems()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription());
    }

    internal static GameObject[] GetValuables()
    {
        return GameObject.FindGameObjectsWithTag(Tag.VALUABLE.GetDescription());
    }

    internal static GameObject GetEnemy()
    {
        return GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
    }

    internal static GameObject[] GetEnemies()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
    }

    internal static GameObject GetAsteroid()
    {
        return GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription());
    }

    internal static GameObject[] GetAsteroids()
    {
        return GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
    }

    internal static PickupClawShooter GetPickupClawShooter()
    {
        var pickupClawShooter = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW_SHOOTER.GetDescription());
        return pickupClawShooter.GetComponent<PickupClawShooter>();
    }

    internal static float GetPickupClawManeuverRadius()
    {
        return GetPickupClawShooter().GetPickupClawPrefab().GetComponent<PickupClawStateMachine>().GetPickupClawManeuverRadius();
    }

    internal static PickupClawStateMachine GetPickupClawStateMachine(GameObject pickupClaw)
    {
        return pickupClaw.GetComponent<PickupClawStateMachine>();
    }

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

    internal static Vector2 GetEnemyPosition()
    {
        TesingWithOneEnemyValidation();

        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.transform.position;
    }

    internal static Vector2 GetItemPosition()
    {
        TesingWithOneItemValidation();

        var item = GameObject.FindGameObjectWithTag(Tag.ITEM.GetDescription());
        return item.transform.position;
    }

    internal static Vector2 GetEnemyMovementDirection()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.GetComponent<Rigidbody2D>().velocity.normalized;
    }
    internal static Vector2 GetEnemyNearestPositionToHalalit()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return GetNearestPositionToHalalit(enemy);
    }

    internal static Vector2 GetAsteroidNearestPositionToHalalit()
    {
        TesingWithOneAsteroidValidation();

        var asteroid = GameObject.FindGameObjectWithTag(Tag.ASTEROID.GetDescription());
        return GetNearestPositionToHalalit(asteroid);
    }

    internal static float GetEnemyHealth()
    {
        TesingWithOneEnemyValidation();
        var enemy = GameObject.FindGameObjectWithTag(Tag.ENEMY.GetDescription());
        return enemy.GetComponent<Health>().GetHealth();
    }

    internal static List<int> GetAllEnemiesHealth()
    {
        TesingWithMultipleEnemiesValidation();
        var enemies = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription()).ToList();
        return enemies.Select(enemy => enemy.GetComponent<Health>().GetHealth()).ToList();
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
        return new Vector2(RandomGenerator.Range(-1f, 1f, true), RandomGenerator.Range(-1f, 1f, true));
    }

    internal static Vector2 GetRandomTouchUnderAttackTrigger(WeaponAttack weaponAttack)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(RandomGenerator.Range(-1f, 1f, true), RandomGenerator.Range(-1f, 1f, true));
        } while (randomTouch.magnitude >= weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }

    internal static Vector2 GetRandomTouchOverAttackTrigger(float attackJoystickEdge)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(RandomGenerator.Range(-1f, 1f, true), RandomGenerator.Range(-1f, 1f, true));
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
    internal static void TesingWithOneEnemyValidation()
    {
        var enemy = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (enemy.Length != 1)
        {
            throw new System.Exception("There should be only one Enemy in the scene");
        }
    }

    internal static void TesingWithOneAsteroidValidation() 
    {
        var asteroid = GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
        if (asteroid.Length != 1)
        {
            throw new System.Exception("There should be only one Asteroid in the scene");
        }
    }

    internal static void TesingWithOneItemValidation()
    {
        var item = GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription());
        if (item.Length != 1)
        {
            throw new System.Exception("There should be only one Item in the scene");
        }
    }

    internal static void TesingWithMultipleEnemiesValidation() 
    {
        var enemies = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        if (enemies.Length <= 1)
        {
            throw new System.Exception("There should be multiple Enemies in the scene");
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
        int hits = 0;
        float newHealth = GetEnemyHealth();
        float lastEnemyHealth = newHealth;
        do
        {
            lastEnemyHealth = GetEnemyHealth();
            lastShotPosition = shot.GetComponent<LineRenderer>().GetPosition(1);
            weaponAttack.HumbleFixedUpdate(attackJoystickTouch);
            yield return null;

            newHealth = GetEnemyHealth();
            if (newHealth < lastEnemyHealth)
            {
                hits++;
            }
        }
        while (hits != expectedHits);

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
        float lastTargetHealth = GetEnemyHealth();
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
            newTargetHealth = GetEnemyHealth();
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