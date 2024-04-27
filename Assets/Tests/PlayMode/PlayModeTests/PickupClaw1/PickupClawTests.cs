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
    public IEnumerator PressOnNothing()
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
    public IEnumerator PressOnJoystick()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var randJoysticksIndex = TestingRandomGenerator.Range(0, 1);

        PickupClawShooter pickupClawShooter = TestUtils.GetPickupClawShooter();
        List<GameObject> joysticks = new List<GameObject>
        {
            TestUtils.GetAttackJoystick(),
            TestUtils.GetMovementJoystick()
        };

        var position = joysticks[randJoysticksIndex].transform.position;
        TestUtils.SetItemPosition(position);
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(position);
        yield return null;

        // THEN
        var pickupClaw = GameObject.FindGameObjectWithTag(Tag.PICKUP_CLAW.GetDescription());
        AssertWrapper.IsNull(pickupClaw, seed);

    }

    [UnityTest]
    public IEnumerator PressOnItemInRange()
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
    public IEnumerator PressOnItemOutOfRange()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        float totalTime = 10f;
        float elapsedTime = 0f;
        float outOfRangeDelta = 0.2f;
        TestUtils.SetRandomItemPosition(maneuverRadius + outOfRangeDelta);
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
        while (pickupClaw != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            state = pickupClawStateMachine.GetState();

            // THEN
            AssertWrapper.IsNotNull(item, seed);
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in pickupClaw NOT null", seed);
            AssertWrapper.AreNotEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Should Not be RETURNING_TO_HALALIT_WITH_TARGET", seed);
        }

    }

    [UnityTest]
    public IEnumerator PressOnItemInRangeThroughObstacles()
    {
        // GIVEN
        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        float totalTime = 10f;
        float elapsedTime = 0f;
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(TestUtils.GetItemPosition());
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET");

        // GIVEN
        var item = TestUtils.GetItem();

        // WHEN
        while (state == PickupClawState.MOVING_TO_TARGET)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state");
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET");

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state");
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING");

        // WHEN
        while (item != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null");
        }

    }

    [UnityTest]
    public IEnumerator PressOnItemWhenItemOnTheWay()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        float totalTime = 10f;
        float elapsedTime = 0f;
        float farItemMutiplier = 1.2f;
        var closeItem = TestUtils.GetItem();
        var closeItemPosition = TestUtils.GetItemPosition();
        var farItemPosition = closeItemPosition * farItemMutiplier;
        var itemDropper = TestUtils.GetItemDropper();
        itemDropper.DropNewItem(ItemName.BALL_SHOT, farItemPosition, Vector2.zero);
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(farItemPosition);
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, seed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", seed);

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
        while (closeItem != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null", seed);
        }

        var farItem = TestUtils.GetItem();
        AssertWrapper.AreEqual(farItem.transform.position, farItemPosition, "Far item changed position" , seed);
        AssertWrapper.IsNotNull(farItem, seed);
    }

    [UnityTest]
    public IEnumerator DropItem()
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
        var itemPosition = TestUtils.GetItemPosition();
        var oppositeVector= Utils.GetOppositeVector(itemPosition.normalized);

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

        // GIVEN
        var halalitMovement = TestUtils.GetHalalitMovement();
        
        //WHEN
        while (state == PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;

            halalitMovement.TryMove(oppositeVector.x, oppositeVector.y, Time.deltaTime);
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", seed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET, "Not RETURNING_TO_HALALIT_WITHOUT_TARGET after RETURNING_TO_HALALIT_WITH_TARGET", seed);
        AssertWrapper.AreNotEqual(Utils.Vector2ToDegrees(TestUtils.GetItemPosition()), Utils.Vector2ToDegrees(itemPosition), "Item did not changed position" , seed);
    }

    [UnityTest]
    public IEnumerator SecondPressWhenClawAlive() 
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;

        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        var itemPosition = TestUtils.GetItemPosition();
        var itemDropper = TestUtils.GetItemDropper();
        var secondItemPosition = Utils.GetOppositeVector(itemPosition);
        itemDropper.DropNewItem(ItemName.BALL_SHOT, secondItemPosition, Vector2.zero);
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(itemPosition);
        yield return null;

        // THEN
        AssertWrapper.IsNotNull(TestUtils.GetPickupClaw(), seed, "Claw did not shoot");

        // WHEN
        pickupClawShooter.TryShootClaw(secondItemPosition);

        var items = TestUtils.GetItems();
        while (items.Length == 2)
        {
            yield return null;
            items = TestUtils.GetItems();
        }

        // THEN
        AssertWrapper.AreEqual(items[0].transform.position, secondItemPosition, "Second item moved", seed);
        
        yield return new WaitForSeconds(1f);

        AssertWrapper.IsNull(TestUtils.GetPickupClaw(), seed, "Second pickupClaw shot");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}