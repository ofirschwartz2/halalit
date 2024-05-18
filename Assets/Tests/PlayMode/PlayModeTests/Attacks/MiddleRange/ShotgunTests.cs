using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ShotgunTests
{
    private const string FUNCTION_SHOOTING_WITH_TARGET_NAME = "ShootingWithTarget";

    [SetUp]
    public void SetUp()
    {
        SeedlessRandomGenerator.SetUseTestingExpectedValue(false);
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_SHOOTING_WITH_TARGET_NAME:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITH_ENEMY_NAME);
                break;
            default:
                SceneManager.LoadScene(TestUtils.TEST_SCENE_WITHOUT_TARGET_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Shooting()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.SHOTGUN);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(AttackName.SHOTGUN);
        var numberOfShots = shotgun.GetComponent<Shotgun>().GetNumberOfShots();

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());

        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", seed);
        AssertWrapper.GreaterOrEqual(shots.Length, numberOfShots.min, "Not Enough Bullets", seed);
        AssertWrapper.GreaterOrEqual(numberOfShots.max, shots.Length, "Too Many Bullets", seed);
    }

    [UnityTest]
    public IEnumerator NumberOfShots() 
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.SHOTGUN);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(AttackName.SHOTGUN).GetComponent<Shotgun>();
        var numberOfShots = shotgun.GetComponent<Shotgun>().GetNumberOfShots();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", seed);
        AssertWrapper.GreaterOrEqual(shots.Length, numberOfShots.min, "Not Enough Bullets", seed);
        AssertWrapper.GreaterOrEqual(numberOfShots.max, shots.Length, "Too Many Bullets", seed);
    }

    [UnityTest]
    public IEnumerator ShotsLifeTime()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.SHOTGUN);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        float firstShotLifeTime = 0, lastShotLifeTime = 0;
        bool firstShotDone = false;

        // WHEN

        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
        var maximumLifeTime = shots[0].GetComponent<ShotgunShot>().GetMaxLifeTime();
        var minimumLifeTime = shots[0].GetComponent<ShotgunShot>().GetMinLifeTime();

        do
        {
            if (!firstShotDone)
            {
                firstShotLifeTime += Time.deltaTime;
            }

            yield return null;

            lastShotLifeTime += Time.deltaTime;

            if (!firstShotDone && shots.Any((shot => shot == null)))
            {
                firstShotDone = true;
            }
        } while (shots.Any(shot => shot != null));

        // THEN
        AssertWrapper.GreaterOrEqual(firstShotLifeTime, minimumLifeTime, "First Shot Ended Too Fast", seed);
        AssertWrapper.GreaterOrEqual(maximumLifeTime, lastShotLifeTime, "Last Shot Ended Too Slow", seed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(AttackName.SHOTGUN);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(AttackName.SHOTGUN).GetComponent<Shotgun>();
        var range = shotgun.GetRange();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", seed);

        var shotgunTopShootingAngle = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, range)));
        var shotgunBottomShootingAngle = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, -range)));

        foreach (var shot in shots)
        {
            var shotDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(shot.transform.rotation));
            AssertWrapper.GreaterOrEqual(shotgunTopShootingAngle, shotDirection, "Shot Outside Range", seed);
            AssertWrapper.GreaterOrEqual(shotDirection, shotgunBottomShootingAngle, "Shot Outside Range", seed);
        }
    }
    
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetUpShot(AttackName.SHOTGUN);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(2); // MUST BE SMALL TO HIT BEFORE MinLifeTime
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetPosition = TestUtils.GetEnemyPosition();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", seed);

        // GIVEN
        var numberOfShots = shots.Length;
        float timeUntilFirstHit = 0;
        var minimumLifeTime = shots[0].GetComponent<ShotgunShot>().GetMinLifeTime();

        // WHEN
        do
        {
            timeUntilFirstHit += Time.deltaTime;
            yield return null;

        } while (shots.Count(shot => shot != null) == numberOfShots);

        AssertWrapper.GreaterOrEqual(minimumLifeTime, timeUntilFirstHit, "Didn't Hit Target Fast As Expected", seed);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}