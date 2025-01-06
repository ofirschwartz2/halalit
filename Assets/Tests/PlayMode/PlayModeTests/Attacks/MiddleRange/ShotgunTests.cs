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
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.SHOTGUN;
    private const Tag ATTACK_TAG = Tag.SHOT;

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
        TestUtils.SetTestMode();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(ATTACK_NAME);
        var numberOfShots = shotgun.GetComponent<Shotgun>().GetNumberOfShots();

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(ATTACK_TAG.GetDescription());

        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", _currentSeed);
        AssertWrapper.GreaterOrEqual(shots.Length, numberOfShots.min, "Not Enough Bullets", _currentSeed);
        AssertWrapper.GreaterOrEqual(numberOfShots.max, shots.Length, "Too Many Bullets", _currentSeed);
    }

    [UnityTest]
    public IEnumerator NumberOfShots() 
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<Shotgun>();
        var numberOfShots = shotgun.GetComponent<Shotgun>().GetNumberOfShots();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot", _currentSeed);
        AssertWrapper.GreaterOrEqual(shots.Length, numberOfShots.min, "Not Enough Bullets", _currentSeed);
        AssertWrapper.GreaterOrEqual(numberOfShots.max, shots.Length, "Too Many Bullets", _currentSeed);
    }

    [UnityTest]
    public IEnumerator ShotsLifeTime()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
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

        var shots = GameObject.FindGameObjectsWithTag(ATTACK_TAG.GetDescription());
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
        AssertWrapper.GreaterOrEqual(firstShotLifeTime, minimumLifeTime, "First Shot Ended Too Fast",_currentSeed);
        AssertWrapper.GreaterOrEqual(maximumLifeTime, lastShotLifeTime, "Last Shot Ended Too Slow",_currentSeed);
    }

    [UnityTest]
    public IEnumerator ShootingInDirection()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        var shotgun = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<Shotgun>();
        var range = shotgun.GetRange();

        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shots = GameObject.FindGameObjectsWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot",_currentSeed);

        var shotgunTopShootingAngle = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, range)));
        var shotgunBottomShootingAngle = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(Utils.GetRotationPlusAngle(weaponMovement.transform.rotation, -range)));

        foreach (var shot in shots)
        {
            var shotDirection = Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(shot.transform.rotation));
            AssertWrapper.GreaterOrEqual(shotgunTopShootingAngle, shotDirection, "Shot Outside Range",_currentSeed);
            AssertWrapper.GreaterOrEqual(shotDirection, shotgunBottomShootingAngle, "Shot Outside Range",_currentSeed);
        }
    }
    
    [UnityTest]
    public IEnumerator ShootingWithTarget()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland();
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
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
        var shots = GameObject.FindGameObjectsWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.GreaterOrEqual(shots.Length, 0, "Didn't Shoot",_currentSeed);

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

        AssertWrapper.GreaterOrEqual(minimumLifeTime, timeUntilFirstHit, "Didn't Hit Target Fast As Expected",_currentSeed);

        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop",_currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}