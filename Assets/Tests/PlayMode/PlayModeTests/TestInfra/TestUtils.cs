using Assets.Enums;
using Assets.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: InternalsVisibleTo("TestsEditMode")]

internal static class TestUtils
{
    #region Constants
    // Scenes:
    public const string PLAYGROUND_SCENE_NAME = "Playground";
    public const string TEST_SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    public const string TEST_SCENE_WITHOUT_TARGET_NAME = "Testing"; 
    public const string TEST_SCENE_FOR_BOUNCES = "TestingForBounces";
    public const string TEST_SCENE_WITH_MANY_ENEMIES_FROM_RIGHT_NAME = "TestingWithManyEnemiesFromRight";
    public const string TEST_SCENE_WITH_ASTEROID_NAME = "TestingWithAsteroid";
    public const string TEST_SCENE_WITH_MANY_ASTEROIDS_FROM_RIGHT_NAME = "TestingWithManyAsteroidsFromRight";
    public const string TEST_SCENE_WITH_ONE_ITEM_NAME = "TestingWithOneItem";
    public const string TEST_SCENE_WITH_VALUABLES_NAME = "TestingWithValuables";
    public const string TEST_SCENE_IGNORING_EDGE_FORCE_FIELDS_OBJECTS_NAME = "TestingIgnoreEdgeForceFields";
    public const string MAIN_MENU_SCENE_NAME= "MainMenu";

    // GameObjects names:
    public const string OBJECT_LOADER_NAME = "ObjectLoader";
    public const string ASTEROID_CONTAINER_NAME = "AsteroidsContainer";
    public const string ENEMIES_CONTAINER_NAME = "EnemiesContainer";
    public const string EXTERNAL_SAFE_ISLAND_NAME = "ExternalSafeIsland";

    // Resources paths:
    public const string SPRITE_CIRCLE_PATH = "Sprites/Circle";

    // Defaults:
    public const int ENEMIES_SEEDED_NUMBERS_LIST_DEFAULT_LENGTH = 5;
    public const int DEFAULT_RADIUS_OF_TARGET_POSITION_AROUND_HALALIT = 5;
    public const ItemRank DEFAULT_ITEM_RANK_1 = ItemRank.COMMON;
    public const ItemRank DEFAULT_ITEM_RANK_2 = ItemRank.RARE;
    public const int DEFAULT_POWER_1 = 1;
    public const int DEFAULT_POWER_2 = 5;
    public const float DEFAULT_CRITICAL_HIT_1 = 0.5f;
    public const float DEFAULT_CRITICAL_HIT_2 = 1.5f;
    public const int DEFAULT_LUCK_1 = 0;
    public const int DEFAULT_LUCK_2 = 100;
    public const float DEFAULT_RATE_1 = 2;
    public const float DEFAULT_RATE_2 = 0.5f;
    public const float DEFAULT_WEIGHT = 0;
    public static readonly AttackStats DEFAULT_ATTACK_STATS_1 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_1, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_2 = new(DEFAULT_ITEM_RANK_2, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_2, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_3 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_2, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_4 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_2, DEFAULT_LUCK_2, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_5 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_1, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly AttackStats DEFAULT_ATTACK_STATS_6 = new(DEFAULT_ITEM_RANK_1, DEFAULT_POWER_2, DEFAULT_CRITICAL_HIT_2, DEFAULT_LUCK_1, DEFAULT_RATE_1, DEFAULT_WEIGHT);
    public static readonly Vector2 DEFAULT_POSITION_TO_THE_RIGHT = new(5, 0);
    #endregion

    #region Turned off game objects
    public static GameObject _asteroidContainer;
    public static GameObject _enemiesContainer;
    #endregion

    #region Random Seeds
    internal static int SetRandomSeed(int seed = 0)
    {
        if (seed == 0)
        {
            seed = TestingRandomGenerator.Range(int.MinValue, int.MaxValue);
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

    internal static void SetEnemyPosition(Vector2 enemyPosition, int index = 0)
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ENEMY.GetDescription());
        target[index].transform.position = enemyPosition;
    }

    internal static void SetRandomEnemyPosition(float radiusOfEnemyPositionAroundHalalit = 5)
    {
        TesingWithOneEnemyValidation();

        SetRandomGameObjectPosition(
            GameObject.FindGameObjectWithTag(
                Tag.ENEMY.GetDescription()), 
                radiusOfEnemyPositionAroundHalalit);
    }

    internal static void SetAsteroidPosition(Vector2 asteroidPosition, int index = 0)
    {
        var target = GameObject.FindGameObjectsWithTag(Tag.ASTEROID.GetDescription());
        SetGameObjectPosition(target[index], asteroidPosition);
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

    internal static void SetItemPosition(Vector2 position, int index = 0) 
    {
        TesingWithOneItemValidation();

        SetGameObjectPosition(
            GameObject.FindGameObjectsWithTag(Tag.ITEM.GetDescription())[index],
            position);
    }

    internal static void SetValuablePosition(Vector2 valuablePosition, int index = 0)
    {
        SetGameObjectPosition(
            GameObject.FindGameObjectsWithTag(Tag.VALUABLE.GetDescription())[index],
            valuablePosition);
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

    internal static void SetTestMode()
    {
        _asteroidContainer = GameObject.Find(ASTEROID_CONTAINER_NAME);
        _asteroidContainer.SetActive(false);

        _enemiesContainer = GameObject.Find(ENEMIES_CONTAINER_NAME);
        _enemiesContainer.SetActive(false);

        var weaponAttack = GetWeaponAttack();
        weaponAttack.SetIsTesting(true);
    }

    #endregion

    #region SceneGetters
    internal static GameObject GetValuablesContainer()
    {
        return GameObject.FindGameObjectWithTag(Tag.VALUABLES_CONTAINER.GetDescription());
    }

    internal static GameObject GetSetup() 
    {
        return GameObject.FindGameObjectWithTag(Tag.SETUP.GetDescription());
    }

    internal static GameObject GetEventSystem() 
    {
        return GameObject.FindGameObjectWithTag(Tag.EVENT_SYSTEM.GetDescription());
    }

    internal static GameObject GetAttacks() 
    {
        return GameObject.FindGameObjectWithTag(Tag.ATTACKS.GetDescription());
    }

    internal static GameObject GetHalalit()
    {
        return GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
    }

    internal static GameObject GetEnemiesContainer() 
    {
        return GameObject.FindGameObjectWithTag(Tag.ENEMIES_CONTAINER.GetDescription());
    }

    internal static GameObject GetAsteroidsContainer()
    {
        return GameObject.FindGameObjectWithTag(Tag.ASTEROIDS_CONTAINER.GetDescription());
    }

    internal static GameObject GetCamera()
    {
        return GameObject.FindGameObjectWithTag(Tag.MAIN_CAMERA.GetDescription());
    }

    internal static int GetScore() 
    {
        var ScoreText = GetScoreText();
        var score = ScoreText.GetComponent<ScoreScript>();
        return score.GetScore();
    }

    internal static ValuableName GetValuableName(GameObject valuable) 
    {
        return valuable.GetComponent<Valuable>().GetValuableName();
    }

    internal static List<KeyValuePair<ValuableName, int>> GetValuableValues() 
    {
        var score = GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription());
        var scoreScript = score.GetComponent<ScoreScript>();
        return scoreScript.GetValuableValues();
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

    internal static GameObject GetInternalWorld() 
    {
        return GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription());
    }

    internal static GameObject GetExternalWorld()
    {
        return GameObject.FindGameObjectWithTag(Tag.EXTERNAL_WORLD.GetDescription());
    }

    internal static GameObject GetExternalTopEdge() 
    {
        return GameObject.FindGameObjectWithTag(Tag.TOP_EDGE.GetDescription());
    }

    internal static GameObject GetExternalBottomEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.BOTTOM_EDGE.GetDescription());
    }

    internal static GameObject GetExternalLeftEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.LEFT_EDGE.GetDescription());
    }

    internal static GameObject GetExternalRightEdge()
    {
        return GameObject.FindGameObjectWithTag(Tag.RIGHT_EDGE.GetDescription());
    }

    internal static GameObject GetAttackJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.ATTACK_JOYSTICK.GetDescription());
    }

    internal static GameObject GetMovementJoystick()
    {
        return GameObject.FindGameObjectWithTag(Tag.MOVEMENT_JOYSTICK.GetDescription());
    }

    internal static GameObject GetScoreText() 
    {
        return GameObject.FindGameObjectWithTag(Tag.SCORE.GetDescription());
    }

    internal static GameObject GetHalalitHealthBar()
    {
        return GameObject.FindGameObjectWithTag(Tag.HALALIT_HEALTH_BAR.GetDescription());
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

    internal static AsteroidMovement GetAsteroidMovement() 
    {
        return GetAsteroid().GetComponent<AsteroidMovement>();
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
        return GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription()).GetComponent<BoxCollider2D>();
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
        return new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
    }

    internal static Vector2 GetRandomTouchUnderAttackTrigger(WeaponAttack weaponAttack)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
        } while (randomTouch.magnitude >= weaponAttack.GetAttackJoystickEdge());

        return randomTouch;
    }

    internal static Vector2 GetRandomTouchOverAttackTrigger(float attackJoystickEdge)
    {
        Vector2 randomTouch;
        do
        {
            randomTouch = new Vector2(TestingRandomGenerator.Range(-1f, 1f, true), TestingRandomGenerator.Range(-1f, 1f, true));
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
    internal static bool IsGameOver()
    {
        return SceneManager.GetActiveScene().name == MAIN_MENU_SCENE_NAME;
    }

    internal static bool IsSomewhereOnInternalWorldEdges(Vector2 position)
    {
        Vector3 position3 = new(position.x, position.y, 1); // TODO: why our InternalWorldBoxCollider2DBoxCollider2D Z=1?

        var boxCollider = GetInternalWorldBoxCollider2D();
        var edgeThreshold = 0.05f;
        var bounds = boxCollider.bounds;

        var largerBounds = new Bounds(bounds.center, bounds.size * (1 + edgeThreshold));
        var smallerBounds = new Bounds(bounds.center, bounds.size * (1 - edgeThreshold));

        return largerBounds.Contains(position3) && !smallerBounds.Contains(position3);
    }

    internal static bool AreCollidersTouch(Collider2D collider1, Collider2D collider2)
    {
        ContactFilter2D contactFilter = new();
        contactFilter.useTriggers = true;

        Collider2D[] results = new Collider2D[10];
        int overlapCount = collider1.OverlapCollider(contactFilter, results);

        bool isColliding = false;
        for (int i = 0; i < overlapCount; i++)
        {
            if (results[i] == collider2)
            {
                isColliding = true;
                break;
            }
        }

        return isColliding;
    }

    internal static bool SomeCollidersTouch(List<Collider2D> colliders)
    {
        foreach (Collider2D collider1 in colliders)
        {
            foreach (Collider2D collider2 in colliders)
            {
                if (collider1 != collider2 && AreCollidersTouch(collider1, collider2))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsPartlyInsideTheWorld(GameObject gameObject)
    {
        var internalWorldCollider = GameObject.FindGameObjectWithTag(Tag.INTERNAL_WORLD.GetDescription()).GetComponent<Collider2D>();
        var gameObjectCollider = gameObject.GetComponent<Collider2D>() != null ?
            gameObject.GetComponent<Collider2D>() : 
            gameObject.GetComponentInChildren<Collider2D>();

        return AreCollidersTouch(gameObjectCollider, internalWorldCollider);
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
        float newHealth;
        float lastEnemyHealth;
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
    #endregion
}