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
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }
    
    [UnityTest]
    public IEnumerator PressOnNothing()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;

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
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;

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
        AssertWrapper.IsNull(pickupClaw, _currentSeed);

    }

    [UnityTest]
    public IEnumerator PressOnItemInRange()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;

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
        AssertWrapper.IsNotNull(pickupClaw, _currentSeed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", _currentSeed);

        // GIVEN
        var item = TestUtils.GetItem();

        // WHEN
        while (state == PickupClawState.MOVING_TO_TARGET) 
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET", _currentSeed);

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING", _currentSeed);

        // WHEN
        while (item != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null", _currentSeed);
        }

    }

    [UnityTest]
    public IEnumerator PressOnItemOutOfRange()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;

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
        AssertWrapper.IsNotNull(pickupClaw, _currentSeed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", _currentSeed);

        // GIVEN
        var item = TestUtils.GetItem();

        // WHEN
        while (pickupClaw != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            state = pickupClawStateMachine.GetState();

            // THEN
            AssertWrapper.IsNotNull(item, _currentSeed);
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in pickupClaw NOT null", _currentSeed);
            AssertWrapper.AreNotEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Should Not be RETURNING_TO_HALALIT_WITH_TARGET", _currentSeed);
        }

    }

    [UnityTest]
    public IEnumerator PressOnItemInRangeThroughObstacles()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetItemPosition(new Vector2(maneuverRadius - 1, 0));
        yield return null;
        TestUtils.SetAsteroidPosition(new Vector2(1.5f, 0), 0);
        TestUtils.SetEnemyPosition(new Vector2(3f, 0), 0);
        yield return null;

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
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;

        var pickupClawShooter = TestUtils.GetPickupClawShooter();
        float totalTime = 10f;
        float elapsedTime = 0f;
        float farItemMutiplier = 1.2f;
        var closeItem = TestUtils.GetItem();
        var closeItemPosition = TestUtils.GetItemPosition();
        var farItemPosition = closeItemPosition * farItemMutiplier;
        var itemDropper = TestUtils.GetItemDropper();
        itemDropper.DropNewItem(ItemName.BALL_SHOT, farItemPosition, Vector2.zero); // TODO: objectLoader.LoadItemInExternalSafeIsland()
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(farItemPosition);
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, _currentSeed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", _currentSeed);

        // WHEN
        while (state == PickupClawState.MOVING_TO_TARGET)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET", _currentSeed);

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING", _currentSeed);

        // WHEN
        while (closeItem != null)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in item NOT null", _currentSeed);
        }

        var farItem = TestUtils.GetItem();
        AssertWrapper.AreEqual(farItem.transform.position, farItemPosition, "Far item changed position" , _currentSeed);
        AssertWrapper.IsNotNull(farItem, _currentSeed);
    }

    [UnityTest]
    public IEnumerator DropItem()
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;
        var maneuverRadius = TestUtils.GetPickupClawManeuverRadius();
        TestUtils.SetRandomItemPosition(maneuverRadius - 1);
        yield return null;
        var pickupClawShooter = TestUtils.GetPickupClawShooter();

        float totalTime = 10f;
        float elapsedTime = 0f;
        yield return null;

        // WHEN
        pickupClawShooter.TryShootClaw(TestUtils.GetItemPosition());
        yield return null;

        // THEN
        var pickupClaw = TestUtils.GetPickupClaw();
        AssertWrapper.IsNotNull(pickupClaw, _currentSeed);
        var pickupClawStateMachine = TestUtils.GetPickupClawStateMachine(pickupClaw);
        var state = pickupClawStateMachine.GetState();
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.MOVING_TO_TARGET, "Not MOVING_TO_TARGET", _currentSeed);

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
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in MOVING_TO_TARGET state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.GRABBING, "Not GRABBING after MOVING_TO_TARGET", _currentSeed);

        // WHEN
        while (state == PickupClawState.GRABBING)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET, "Not RETURNING_TO_HALALIT_WITH_TARGET after GRABBING", _currentSeed);

        // GIVEN
        var halalitMovement = TestUtils.GetHalalitMovement();
        
        //WHEN
        while (state == PickupClawState.RETURNING_TO_HALALIT_WITH_TARGET)
        {
            yield return null;
            state = pickupClawStateMachine.GetState();
            elapsedTime += Time.deltaTime;

            halalitMovement.TryMove(oppositeVector.x, oppositeVector.y, Time.deltaTime);
            AssertWrapper.Greater(totalTime, elapsedTime, "Stuck in GRABBING state", _currentSeed);
        }

        // THEN
        AssertWrapper.AreEqual((int)state, (int)PickupClawState.RETURNING_TO_HALALIT_WITHOUT_TARGET, "Not RETURNING_TO_HALALIT_WITHOUT_TARGET after RETURNING_TO_HALALIT_WITH_TARGET", _currentSeed);
        AssertWrapper.AreNotEqual(Utils.Vector2ToDegrees(TestUtils.GetItemPosition()), Utils.Vector2ToDegrees(itemPosition), "Item did not changed position" , _currentSeed);
    }

    [UnityTest]
    public IEnumerator SecondPressWhenClawAlive() 
    {
        // GIVEN
        TestUtils.SetTestMode();
        yield return null;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadItemInExternalSafeIsland(ItemName.ITEM_BASE);
        yield return null;

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
        AssertWrapper.IsNotNull(TestUtils.GetPickupClaw(), _currentSeed, "Claw did not shoot");

        // WHEN
        pickupClawShooter.TryShootClaw(secondItemPosition);

        var items = TestUtils.GetItems();
        while (items.Length == 2)
        {
            yield return null;
            items = TestUtils.GetItems();
        }

        // THEN
        AssertWrapper.AreEqual(items[0].transform.position, secondItemPosition, "Second item moved", _currentSeed);
        
        yield return new WaitForSeconds(1f);

        AssertWrapper.IsNull(TestUtils.GetPickupClaw(), _currentSeed, "Second pickupClaw shot");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}