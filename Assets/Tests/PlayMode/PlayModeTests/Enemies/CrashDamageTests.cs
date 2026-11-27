using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CrashDamageTests
{
    private const int ENEMY_HEALTH = 100;
    private const int HALALIT_HEALTH = 1000;
    private const int KILLABLE_ENEMY_HEALTH = 2;
    private const float BELOW_CRASH_SPEED = 5.5f;
    private const float LOWEST_CRASH_SPEED = 6.5f;
    private const float MIDDLE_CRASH_SPEED = 8.5f;
    private const float TOP_CRASH_SPEED = 14f;
    private const int LOWEST_CRASH_HARM = 2;
    private const int MIDDLE_CRASH_HARM = 3;
    private const int TOP_CRASH_HARM = 5;
    private const float HEAD_ON_SPEED = 4.5f;
    private const float SLOW_BUMP_SPEED = 4f;
    private const float SHIP_CRASH_SPEED = 10f;
    private const float CRASH_COOLDOWN = 0.5f;
    private const float COOLDOWN_MARGIN = 0.2f;
    private const float START_GAP = 0.5f;
    private const float MAX_PUSH_TIME = 2f;
    private const int SETTLE_STEPS = 5;
    private const float HEAD_ON_CENTERS_DISTANCE = 3f;
    private static readonly Vector2 ASTEROID_POSITION = TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + Vector2.right * 3;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator EveryEnemyPrefabHasCrashDamage()
    {
        // GIVEN
        yield return null;

        // WHEN
        var prefabs = Resources.LoadAll<GameObject>(TestUtils.ENEMIES_PREFABS_PATH);

        // THEN
        foreach (var prefab in prefabs)
        {
            if (prefab.GetComponent<EnemySharedBehavior>() == null)
            {
                continue;
            }

            AssertWrapper.IsNotNull(prefab.GetComponent<CrashDamage>(), _currentSeed, $"{prefab.name} has no CrashDamage (Needs Ofir in Unity, #232)");
            AssertWrapper.IsNotNull(prefab.GetComponent<Knockbackee>(), _currentSeed, $"{prefab.name} has no Knockbackee, which CrashDamage reads its speed from (Needs Ofir in Unity, #232)");
        }
    }

    [UnityTest]
    public IEnumerator CrashHarmGrowsWithSpeed()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var crashDamage = TestUtils.GetCrashDamage(enemy);
        var health = enemy.GetComponent<Health>();
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        crashDamage.TryCrash(BELOW_CRASH_SPEED);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH, health.GetHealth(), "A crash below the crash speed hurt the enemy", _currentSeed);

        // WHEN
        crashDamage.TryCrash(LOWEST_CRASH_SPEED);

        // THEN
        var expectedHealth = ENEMY_HEALTH - LOWEST_CRASH_HARM;
        AssertWrapper.AreEqual(expectedHealth, health.GetHealth(), "A crash just over the crash speed did not take the base harm", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(CRASH_COOLDOWN + COOLDOWN_MARGIN);
        crashDamage.TryCrash(MIDDLE_CRASH_SPEED);

        // THEN
        expectedHealth -= MIDDLE_CRASH_HARM;
        AssertWrapper.AreEqual(expectedHealth, health.GetHealth(), "A faster crash did not take one more harm", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(CRASH_COOLDOWN + COOLDOWN_MARGIN);
        crashDamage.TryCrash(TOP_CRASH_SPEED);

        // THEN
        expectedHealth -= TOP_CRASH_HARM;
        AssertWrapper.AreEqual(expectedHealth, health.GetHealth(), "A very fast crash did not take the most harm", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CrashHurtsOncePerCooldown()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var crashDamage = TestUtils.GetCrashDamage(enemy);
        var health = enemy.GetComponent<Health>();
        yield return null;
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        crashDamage.TryCrash(MIDDLE_CRASH_SPEED);
        crashDamage.TryCrash(MIDDLE_CRASH_SPEED);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - MIDDLE_CRASH_HARM, health.GetHealth(), "Two crashes at the same moment hurt the enemy twice", _currentSeed);

        // WHEN
        yield return new WaitForSeconds(CRASH_COOLDOWN + COOLDOWN_MARGIN);
        crashDamage.TryCrash(MIDDLE_CRASH_SPEED);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - 2 * MIDDLE_CRASH_HARM, health.GetHealth(), "A crash after the cooldown did not hurt the enemy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CrashIntoAsteroidHurts()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetCrashDamage(enemy);
        var asteroidCollider = LoadStillAsteroid(ASTEROID_POSITION).GetComponent<Collider2D>();
        yield return null;
        yield return new WaitForFixedUpdate();
        StopAsteroidRotation();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        PlaceLeftOf(enemy, asteroidCollider);

        // WHEN
        yield return PushUntilTouching(asteroidCollider, enemy, Vector2.right * MIDDLE_CRASH_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - MIDDLE_CRASH_HARM, enemy.GetComponent<Health>().GetHealth(), "Enemy slammed into an asteroid did not take the crash harm", _currentSeed);
    }

    [UnityTest]
    public IEnumerator HeadOnEnemiesBothHurt()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var leftEnemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var rightEnemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT + Vector2.right * HEAD_ON_CENTERS_DISTANCE);
        TestUtils.GetCrashDamage(leftEnemy);
        TestUtils.GetCrashDamage(rightEnemy);
        yield return null;
        yield return new WaitForFixedUpdate();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);

        // WHEN
        yield return PushUntilTouching(rightEnemy.GetComponent<Collider2D>(), leftEnemy, Vector2.right * HEAD_ON_SPEED, rightEnemy, Vector2.left * HEAD_ON_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - MIDDLE_CRASH_HARM, leftEnemy.GetComponent<Health>().GetHealth(), "Left enemy in a head-on crash did not take the crash harm of the two speeds together", _currentSeed);
        AssertWrapper.AreEqual(ENEMY_HEALTH - MIDDLE_CRASH_HARM, rightEnemy.GetComponent<Health>().GetHealth(), "Right enemy in a head-on crash did not take the crash harm of the two speeds together", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CrashIntoEdgeHurts()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetCrashDamage(enemy);
        var rightEdgeCollider = TestUtils.GetExternalRightEdge().GetComponents<Collider2D>().First(collider => !collider.isTrigger);
        yield return null;
        yield return new WaitForFixedUpdate();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        PlaceLeftOf(enemy, rightEdgeCollider);

        // WHEN
        yield return PushUntilTouching(rightEdgeCollider, enemy, Vector2.right * MIDDLE_CRASH_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - MIDDLE_CRASH_HARM, enemy.GetComponent<Health>().GetHealth(), "Enemy slammed into the world edge did not take the crash harm", _currentSeed);
    }

    [UnityTest]
    public IEnumerator SlowBumpNoHarm()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetCrashDamage(enemy);
        var asteroidCollider = LoadStillAsteroid(ASTEROID_POSITION).GetComponent<Collider2D>();
        yield return null;
        yield return new WaitForFixedUpdate();
        StopAsteroidRotation();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        PlaceLeftOf(enemy, asteroidCollider);

        // WHEN
        yield return PushUntilTouching(asteroidCollider, enemy, Vector2.right * SLOW_BUMP_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH, enemy.GetComponent<Health>().GetHealth(), "A slow bump into an asteroid hurt the enemy", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DisabledCrashDamageNoHarm()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetCrashDamage(enemy).enabled = false;
        var asteroidCollider = LoadStillAsteroid(ASTEROID_POSITION).GetComponent<Collider2D>();
        yield return null;
        yield return new WaitForFixedUpdate();
        StopAsteroidRotation();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        PlaceLeftOf(enemy, asteroidCollider);

        // WHEN
        yield return PushUntilTouching(asteroidCollider, enemy, Vector2.right * MIDDLE_CRASH_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH, enemy.GetComponent<Health>().GetHealth(), "An enemy with CrashDamage switched off was hurt by a crash", _currentSeed);
    }

    [UnityTest]
    public IEnumerator CrashKillsEnemy()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        var crashDamage = TestUtils.GetCrashDamage(enemy);
        yield return null;
        TestUtils.SetEnemiesHealth(KILLABLE_ENEMY_HEALTH);

        // WHEN
        crashDamage.TryCrash(LOWEST_CRASH_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);
        yield return null;

        // THEN
        AssertWrapper.IsTrue(enemy == null, "A crash that took the enemy's last health did not destroy it", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ThrownEnemyIntoShipDoesTouchDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var enemy = LoadPlainEnemy(TestUtils.DEFAULT_POSITION_TO_THE_RIGHT);
        TestUtils.GetCrashDamage(enemy);
        var halalit = TestUtils.GetHalalit();
        var halalitHealth = halalit.GetComponent<Health>();
        var halalitCollider = halalit.GetComponent<Collider2D>();
        var touchHarm = enemy.GetComponent<FixedCollisionHarmer>().GetCollisionHarm();
        yield return null;
        yield return new WaitForFixedUpdate();
        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        halalitHealth.SetHealth(HALALIT_HEALTH);
        PlaceRightOf(enemy, halalitCollider);

        // WHEN
        yield return PushUntilTouching(halalitCollider, enemy, Vector2.left * SHIP_CRASH_SPEED);
        yield return WaitFixedSteps(SETTLE_STEPS);

        // THEN
        AssertWrapper.AreEqual(HALALIT_HEALTH - touchHarm, halalitHealth.GetHealth(), "Enemy thrown into the Halalit did not do exactly its touch damage", _currentSeed);
        AssertWrapper.AreEqual(ENEMY_HEALTH, enemy.GetComponent<Health>().GetHealth(), "Enemy thrown into the Halalit was hurt by the crash", _currentSeed);
    }

    // The plain enemy from the scene's ObjectLoader (a Lame Enemy, its movement stopped), placed at position.
    private GameObject LoadPlainEnemy(Vector2 position)
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var enemy = objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetGameObjectPosition(enemy, position);
        return enemy;
    }

    // The asteroid from the scene's ObjectLoader (speed 0), placed at position. Its rotation is set in its Start: stop it after a frame.
    private GameObject LoadStillAsteroid(Vector2 position)
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var asteroid = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        TestUtils.SetGameObjectPosition(asteroid, position);
        return asteroid;
    }

    // With no speed and no rotation the asteroid's own speed harm on the enemy is 0, so only the crash can hurt it.
    private void StopAsteroidRotation()
    {
        TestUtils.GetAsteroidMovement().SetRotationSpeed(0);
    }

    // Places the enemy just left of the target, on the target's height, START_GAP away.
    private void PlaceLeftOf(GameObject enemy, Collider2D target)
    {
        var enemyHalfWidth = enemy.GetComponent<Collider2D>().bounds.extents.x;
        TestUtils.SetGameObjectPosition(enemy, new Vector2(target.bounds.min.x - enemyHalfWidth - START_GAP, target.bounds.center.y));
    }

    // Places the enemy just right of the target, on the target's height, START_GAP away.
    private void PlaceRightOf(GameObject enemy, Collider2D target)
    {
        var enemyHalfWidth = enemy.GetComponent<Collider2D>().bounds.extents.x;
        TestUtils.SetGameObjectPosition(enemy, new Vector2(target.bounds.max.x + enemyHalfWidth + START_GAP, target.bounds.center.y));
    }

    // Sets the movers' velocities before every physics step until the mover touches the target. Enemies have linear drag,
    // so this keeps the speed going into the crash exact.
    private IEnumerator PushUntilTouching(Collider2D target, GameObject mover, Vector2 velocity, GameObject otherMover = null, Vector2 otherVelocity = default)
    {
        var moverRigidbody = mover.GetComponent<Rigidbody2D>();
        var moverCollider = mover.GetComponent<Collider2D>();
        var otherMoverRigidbody = otherMover != null ? otherMover.GetComponent<Rigidbody2D>() : null;
        var pushStartTime = Time.time;

        while (!moverCollider.IsTouching(target) && Time.time < pushStartTime + MAX_PUSH_TIME)
        {
            moverRigidbody.velocity = velocity;
            if (otherMoverRigidbody != null)
            {
                otherMoverRigidbody.velocity = otherVelocity;
            }
            yield return new WaitForFixedUpdate();
        }

        AssertWrapper.IsTrue(moverCollider.IsTouching(target), $"{mover.name} never reached {target.name}", _currentSeed);
    }

    private IEnumerator WaitFixedSteps(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
