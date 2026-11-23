using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BoomerangTests
{

    private const AttackName SHOT_NAME = AttackName.BOOMERANG;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(AttackName.BOOMERANG);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingWithoutTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        
        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 firstShotPosition = shot.transform.position;
        Vector2 lastShotPosition;
        var acceptedDelta = 0.5f;

        // WHEN
        do {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(firstShotPosition.x, lastShotPosition.x, "Boomerang Ended Not Where It Started", seed, acceptedDelta);
        AssertWrapper.AreEqual(firstShotPosition.y, lastShotPosition.y, "Boomerang Ended Not Where It Started", seed, acceptedDelta);
    }

    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN

        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetUpShot(AttackName.BOOMERANG);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 2f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", _currentSeed, acceptedDelta);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShotLifetime()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        float supposedLifetime = shot.GetComponent<Boomerang>().GetLifetime();
        float actualShotTime = 0;

        // WHEN
        do
        {
            actualShotTime += Time.deltaTime;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN
        AssertWrapper.AreEqual(supposedLifetime, actualShotTime, "Shot Lifetime Not As Expected", seed, acceptedDelta);

    }

    private const float FETCH_MIN_DISTANCE_FROM_HALALIT = 2f;
    [UnityTest]
    public IEnumerator FetchesValuableToHalalit()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var halalit = TestUtils.GetHalalit();
        var originalScore = TestUtils.GetScore();
        var goldValue = TestUtils.GetValuableValues().Find(valuableValue => valuableValue.Key == ValuableName.GOLD).Value;
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        while (shot != null && Vector2.Distance(shot.transform.position, halalit.transform.position) <= FETCH_MIN_DISTANCE_FROM_HALALIT)
        {
            yield return null;
        }
        AssertWrapper.IsNotNull(shot, _currentSeed, "Shot Ended Before Getting Far From Halalit");

        // WHEN
        IEnumerator drop = TestUtils.DropValuable(ValuableName.GOLD, shot.transform.position);
        while (drop.MoveNext())
        {
            yield return null;
        }

        while (shot != null)
        {
            yield return null;
        }
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.AreEqual(0, TestUtils.GetValuables().Length, "Valuable Was Not Brought To Halalit", _currentSeed);
        AssertWrapper.AreEqual(originalScore + goldValue, TestUtils.GetScore(), "Score Did Not Rise By The Gold Value", _currentSeed);
    }

    [UnityTest]
    public IEnumerator DropsValuableOnEnemy()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        TestUtils.SetUpShot(AttackName.BOOMERANG);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        var halalit = TestUtils.GetHalalit();
        var originalScore = TestUtils.GetScore();
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        var shot = TestUtils.GetShot();
        AssertWrapper.IsNotNull(shot, _currentSeed);

        while (shot != null && Vector2.Distance(shot.transform.position, halalit.transform.position) <= FETCH_MIN_DISTANCE_FROM_HALALIT)
        {
            yield return null;
        }
        AssertWrapper.IsNotNull(shot, _currentSeed, "Shot Ended Before Getting Far From Halalit");

        // WHEN
        IEnumerator drop = TestUtils.DropValuable(ValuableName.GOLD, shot.transform.position);
        while (drop.MoveNext())
        {
            yield return null;
        }
        var valuable = (GameObject)drop.Current;

        while (shot != null)
        {
            yield return null;
        }
        yield return null;
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(valuable, _currentSeed, "Valuable Was Destroyed With The Boomerang");
        AssertWrapper.AreEqual(originalScore, TestUtils.GetScore(), "Score Changed Though The Boomerang Hit An Enemy", _currentSeed);
        AssertWrapper.Greater(Vector2.Distance(valuable.transform.position, halalit.transform.position), FETCH_MIN_DISTANCE_FROM_HALALIT, "Valuable Was Brought To Halalit Though The Boomerang Hit An Enemy", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}