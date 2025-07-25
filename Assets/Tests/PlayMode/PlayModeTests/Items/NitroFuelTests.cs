using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class NitroFuelTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator NitroFuelMakesHalalitFaster()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var nitroFuel = objectLoader.LoadItemInExternalSafeIsland(ItemName.NITRO_FUEL);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var initialMaxSpeed = halalitMovement.GetSpeedLimit();

        // Move left for 5 seconds to see max speed without NitroFuel
        var leftMovement = Vector2.left;
        for (float t = 0; t < 5f; t += Time.deltaTime)
        {
            halalitMovement.TryMove(leftMovement.x, leftMovement.y, Time.deltaTime);
            yield return null;
        }

        var speedWithoutNitro = halalitMovement.GetComponent<Rigidbody2D>().velocity.magnitude;

        // Move NitroFuel to origin
        TestUtils.SetItemPosition(Vector2.zero);
        yield return null;

        // Move right until NitroFuel is picked up or 5 seconds pass
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (nitroFuel != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        AssertWrapper.IsTrue(nitroFuel == null, "Failed to pick up NitroFuel within 5 seconds", _currentSeed);

        // Click utility button to activate NitroFuel
        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();
        var clicked = false;
        UnityEngine.Events.UnityAction clickAction = () => clicked = true;
        utilityButton.AddClickListener(clickAction);
        clickAction.Invoke();  // Simulate the click
        yield return null;
        utilityButton.RemoveClickListener(clickAction);

        // Move left for 5 seconds to see max speed with NitroFuel
        for (float t = 0; t < 5f; t += Time.deltaTime)
        {
            halalitMovement.TryMove(leftMovement.x, leftMovement.y, Time.deltaTime);
            yield return null;
        }

        var speedWithNitro = halalitMovement.GetComponent<Rigidbody2D>().velocity.magnitude;

        // THEN
        AssertWrapper.Greater(speedWithNitro, speedWithoutNitro, "NitroFuel did not increase Halalit's speed", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
} 