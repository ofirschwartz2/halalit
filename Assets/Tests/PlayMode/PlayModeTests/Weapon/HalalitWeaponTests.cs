using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class HalalitWeaponTests
{

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator JoystickUnderAttackTrigger()
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchUnderAttackTrigger(weaponAttack);

        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        // THEN
        AssertWrapper.IsNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator JoystickOverAttackTriggerShooting()
    {
        // GIVEN
        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        // THEN
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator DelayBetweenShotsLargerThanCooldown() 
    {
        // GIVEN
        float totalTime = 3f;
        float elapsedTime = 0f;
        int shotsCount = 0;
        float firstShotTime = 0f;

        TestUtils.SetUpShot(AttackName.BALL_SHOT);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        while (elapsedTime < totalTime)
        {
            weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

            var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
            if (shots.Length > shotsCount)
            {
                if (shots.Length == 1) // 1st shot fired
                {
                    firstShotTime = elapsedTime;
                    shotsCount = shots.Length;
                }
                else if (shots.Length == 2) // 2nd shot fired
                {
                    // THEN
                    AssertWrapper.GreaterOrEqual(elapsedTime - firstShotTime, weaponAttack.GetCooldownInterval(), "Delay Under Cooldown", _currentSeed);
                    break;
                }
                else
                {
                    // THEN
                    AssertWrapper.Fail(">2 Shots Fired", _currentSeed);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator JoystickDirectionToWeaponDirection()
    {
        // GIVEN
        var acceptedDelta = 0.1f;
        var weaponMovement = TestUtils.GetWeaponMovement();

        Vector2 randomTouchOnAttackJoystick = Vector2.zero;
        while (randomTouchOnAttackJoystick == Vector2.zero)
        {
            randomTouchOnAttackJoystick = TestUtils.GetRandomTouch();
        } 
        
        // WHEN
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);

        yield return null; // TODO: WHEN WE HAVE WEAPON DRAG, WAIT FOR X SECONDS

        // THEN
        AssertWrapper.AreEqual(
            Utils.Vector2ToDegrees(randomTouchOnAttackJoystick),
            Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation)),
            "Joystick vs Weapon Direction",
            _currentSeed,
            acceptedDelta
            );
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}