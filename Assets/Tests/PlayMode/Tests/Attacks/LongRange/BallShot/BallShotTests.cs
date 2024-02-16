using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class BallShotTests
{

    private const string SCENE_NAME = "Playground";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
        
    }

    [UnityTest]
    public IEnumerator BallShotShooting()
    {
        TestUtils.SetUpBallShot();

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot);
    }


    [UnityTest]
    public IEnumerator BallShotShootingWithoutTarget()
    {
        TestUtils.SetUpBallShot();

        var weaponAttack = TestUtils.GetWeaponAttack();

        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);

        yield return null;

        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());

        Assert.IsNotNull(shot); // There is a shot

        Vector2 lastShotPosition;

        do {
            lastShotPosition = shot.transform.position;
            yield return null;
            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        Assert.IsTrue(TestUtils.IsSomewhereOnInternalWorldEdges(lastShotPosition));

        // wait until the shot collides with something

    }

}