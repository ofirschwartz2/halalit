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
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        float totalTime = 10f;
        float elapsedTime = 0f;
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(TestUtils.GetItemPosition());
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, seed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", seed);

        // GIVEN
        var item = TestUtils.GetItem();

        // WHEN
        while (state == PickupClawState.MOVING_TO_TARGET) 
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state", seed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET", seed);

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", seed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING", seed);

        // WHEN
        while (item != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null", seed);
        }

    }

    [UnityTest]
    public IEnumerator ClawPressOnItemOutOfRange()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        float totalTime = 10f;
        float elapsedTime = 0f;
        TestUtils.SetRandomItemPosition(maneuverRadius + 1);
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(TestUtils.GetItemPosition());
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, seed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", seed);

        // GIVEN
        var item = TestUtils.GetItem();

        // WHEN
        while (state == PickupClawState.MOVING_TO_TARGET)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state", seed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET", seed);

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", seed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING", seed);

        // WHEN
        while (item != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null", seed);
        }

    }

}