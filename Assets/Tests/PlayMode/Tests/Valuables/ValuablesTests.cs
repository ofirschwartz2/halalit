using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class ValuablesTests
{

    private const string SCENE_WITH_ENEMY_NAME = "TestingWithEnemy";
    private const string SCENE_WITH_ASTEROID_NAME = "TestingWithAsteroid";


    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_WITH_ENEMY_NAME);
    }
    
    
    [UnityTest]
    public IEnumerator EnemyDrop1Or0Valuables()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        TestUtils.SetEnemiesSeededNumbers();
        TestUtils.SetEnemiesHealth(1);

        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition();
        yield return null;

        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var acceptedDelta = 0.5f;
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        yield return null;
        yield return null;
        yield return null;
        yield return null;


        // THEN
        AssertWrapper.IsNull(TestUtils.GetEnemies);
        AssertWrapper.AreEqual(lastShotPosition.x, targetClosestPosition.x, "Shot Didn't Hit Target", seed, acceptedDelta);
        AssertWrapper.AreEqual(lastShotPosition.y, targetClosestPosition.y, "Shot Didn't Hit Target", seed, acceptedDelta);
    }
}