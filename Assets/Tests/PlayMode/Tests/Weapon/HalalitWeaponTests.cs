using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class HalalitWeaponTests
{

    private const string SCENE_NAME = "Testing";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator JoystickUnderAttackTrigger()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchUnderAttackTrigger(weaponAttack);

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());        

        Assert.IsNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator JoystickOverAttackTriggerShooting()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot, $"seed: {seed}");
    }

    [UnityTest]
    public IEnumerator DelayBetweenShotsLargerThanCooldown() 
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);
        TestUtils.SetUpShot(AttackName.BALL_SHOT);

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        float totalTime = 3f;
        float elapsedTime = 0f;
        int shotsCount = 0;
        float firstShotTime = 0f;

        while (elapsedTime < totalTime)
        {
            weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

            var shots = GameObject.FindGameObjectsWithTag(Tag.SHOT.GetDescription());
            if (shots.Length > shotsCount)
            {
                if (shots.Length == 1) // First shot fired
                {
                    firstShotTime = elapsedTime;
                    shotsCount = shots.Length;
                }
                else if (shots.Length == 2) // Second shot fired
                {
                    Assert.GreaterOrEqual(elapsedTime - firstShotTime, weaponAttack.GetCooldownInterval(), $"seed: {seed}");
                    break;
                }
                else 
                {
                    Assert.Fail($"Seed: {seed}");
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    [UnityTest]
    public IEnumerator JoystickDirectionToWeaponDirection()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var acceptedDelta = 0.1f;

        Vector2 randomTouchOnAttackJoystick = Vector2.zero;
        while (randomTouchOnAttackJoystick == Vector2.zero)
        {
            randomTouchOnAttackJoystick = TestUtils.GetRandomTouch();
        } 
        
        weaponMovement.TryChangeWeaponPosition(randomTouchOnAttackJoystick);

        yield return null; // TODO: WHEN WE HAVE WEAPON DRAG WAIT FOR X SECONDS

        Assert.AreEqual(
            Utils.Vector2ToDegrees(randomTouchOnAttackJoystick),
            Utils.Vector2ToDegrees(Utils.GetRotationAsVector2(weaponMovement.transform.rotation)),
            acceptedDelta,
            $"seed: {seed}"
            );
    }

}