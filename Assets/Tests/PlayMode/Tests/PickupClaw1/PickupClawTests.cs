using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class PickupClawTests
{

    private const string SCENE_NAME = "TestingWithOneItem";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }
    
    
    [UnityTest]
    public IEnumerator ClawPressOnNothing()
    {
        // GIVEN
        PickupClawShooter pickupClawShooter = TestUtils.GetPickupClawShooter();
        var position = new Vector2(-1, 0);

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        AssertWrapper.IsNull(pickupClaw);
        
    }

    [UnityTest]
    public IEnumerator ClawPressOnJoystick()
    {
        // GIVEN
        PickupClawShooter pickupClawShooter = TestUtils.GetPickupClawShooter();
        GameObject attackJoystick = TestUtils.GetAttackJoystick();
        
        var position = attackJoystick.transform.position;

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        AssertWrapper.IsNull(pickupClaw);

    }

    [UnityTest]
    public IEnumerator ClawPressOnItemInRange()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        var maneuverRadius = pickupClawStateMachine.GetPickupClawManeuverRadius();

        TestUtils.SetRandomItemPosition(maneuverRadius - 1);

        
        // WHEN
        pickupClawShooter.TryShootClaw(TestUtils.GetItemPosition());
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, seed);

        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.IsNotNull(state, seed);
        while (state != PickupClawState)

    }

}