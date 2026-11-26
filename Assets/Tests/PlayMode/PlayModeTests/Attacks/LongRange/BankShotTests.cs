using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class BankShotTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.BANK_SHOT;
    private const int ENEMY_HEALTH = 100;
    private const int ASTEROID_HEALTH = 100;
    private const float TARGET_DISTANCE = 4f;
    private const float WALL_DISTANCE = 4f;
    private const float DIRECT_HIT_MULTIPLIER = 0.5f;
    private const float FIRST_BOUNCE_MULTIPLIER = 2f;
    private const float THIRD_BOUNCE_MULTIPLIER = 4f;
    private const float LIFETIME_DELTA = 0.3f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator DirectHitDoesHalfDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        Vector2 halalitPosition = GetWorldBounds().center;
        yield return PlaceHalalit(halalitPosition);
        TestUtils.SetEnemyPosition(halalitPosition + Vector2.right * TARGET_DISTANCE);
        yield return null;

        // WHEN
        yield return FireRight();
        AssertWrapper.IsNotNull(TestUtils.GetShot(), _currentSeed);

        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - GetExpectedHarm(DIRECT_HIT_MULTIPLIER), TestUtils.GetEnemyHealth(), "Direct Hit Did Not Do Half Damage, Rounded Up", _currentSeed);
    }

    [UnityTest]
    public IEnumerator WallBounceDoublesDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var halalitPosition = GetPositionNearRightWall();
        yield return PlaceHalalit(halalitPosition);
        TestUtils.SetEnemyPosition(halalitPosition + Vector2.left * TARGET_DISTANCE);
        yield return null;

        // WHEN
        yield return FireRight();
        AssertWrapper.IsNotNull(TestUtils.GetShot(), _currentSeed);

        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - GetExpectedHarm(FIRST_BOUNCE_MULTIPLIER), TestUtils.GetEnemyHealth(), "Hit After One Wall Bounce Did Not Do Double Damage", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ThirdBounceHitDoesFourTimesDamage()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetEnemiesHealth(ENEMY_HEALTH);
        var halalitPosition = GetPositionNearRightWall();
        yield return PlaceHalalit(halalitPosition);
        TestUtils.SetEnemyPosition(halalitPosition + Vector2.left * TARGET_DISTANCE);
        yield return null;

        // WHEN
        yield return FireRight();
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);
        shot.GetComponent<BankShot>().SetBounces(2);

        yield return WaitUntilShotIsGone();

        // THEN
        AssertWrapper.AreEqual(ENEMY_HEALTH - GetExpectedHarm(THIRD_BOUNCE_MULTIPLIER), TestUtils.GetEnemyHealth(), "Hit After The Third Bounce Did Not Do Four Times Damage", _currentSeed);
    }

    [UnityTest]
    public IEnumerator FourthBounceRemovesDisc()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var halalitPosition = GetPositionNearRightWall();
        yield return PlaceHalalit(halalitPosition);

        // WHEN
        yield return FireRight();
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);
        shot.GetComponent<BankShot>().SetBounces(3);

        Vector2 lastShotPosition;
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = TestUtils.GetShot();
        } while (shot != null);

        // THEN
        AssertWrapper.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition), "Disc Was Not Removed At The Wall", _currentSeed);
        AssertWrapper.Greater(lastShotPosition.x, halalitPosition.x, "Disc Came Back Past The Halalit After Its Fourth Bounce", _currentSeed);
    }

    [UnityTest]
    public IEnumerator AsteroidIsBumperNotTarget()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetAsteroidsHealth(ASTEROID_HEALTH);
        Vector2 halalitPosition = GetWorldBounds().center;
        yield return PlaceHalalit(halalitPosition);
        TestUtils.SetAsteroidPosition(halalitPosition + Vector2.right * TARGET_DISTANCE);
        yield return null;

        // WHEN
        yield return FireRight();
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);
        var shotRigidBody = shot.GetComponent<Rigidbody2D>();
        var waitEnd = Time.time + shot.GetComponent<BankShot>().GetLifeTime();

        do
        {
            yield return null;
        } while (shot != null && shotRigidBody.velocity.x > 0 && Time.time < waitEnd);

        yield return new WaitForFixedUpdate();
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(shot, _currentSeed, "Asteroid Removed The Disc");
        AssertWrapper.Less(shotRigidBody.velocity.x, 0, "Disc Did Not Bounce Back Toward The Halalit", _currentSeed);
        AssertWrapper.AreEqual(ASTEROID_HEALTH, TestUtils.GetAsteroidHealth(), "Disc Harmed The Asteroid", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DiscLivesFourSeconds()
    {
        // GIVEN
        yield return null;
        TestUtils.AssertAttackPrefabHas<BankShot>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
        yield return PlaceHalalit(GetWorldBounds().center);

        // WHEN
        yield return FireRight();
        var fireTime = Time.time;
        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);
        var lifetime = shot.GetComponent<BankShot>().GetLifeTime();

        while (shot != null)
        {
            yield return null;
            shot = TestUtils.GetShot();
        }

        // THEN
        AssertWrapper.AreEqual(lifetime, Time.time - fireTime, "Disc Did Not Live For Its Lifetime", _currentSeed, LIFETIME_DELTA);
    }

    private Bounds GetWorldBounds()
    {
        return TestUtils.GetInternalWorldBoxCollider2D().bounds;
    }

    private Vector2 GetPositionNearRightWall()
    {
        var worldBounds = GetWorldBounds();
        return new Vector2(worldBounds.max.x - WALL_DISTANCE, worldBounds.center.y);
    }

    private int GetExpectedHarm(float multiplier)
    {
        return Mathf.CeilToInt(TestUtils.DEFAULT_ATTACK_STATS_2.Power * multiplier);
    }

    private IEnumerator PlaceHalalit(Vector2 position)
    {
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), position);
        yield return new WaitForFixedUpdate();
        yield return null;
    }

    private IEnumerator FireRight()
    {
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(Vector2.right, weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;
    }

    private IEnumerator WaitUntilShotIsGone()
    {
        var shot = TestUtils.GetShot();
        while (shot != null)
        {
            yield return null;
            shot = TestUtils.GetShot();
        }

        yield return new WaitForFixedUpdate();
        yield return null;
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
