using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class HalalitMovementTests
{

    private const string SCENE_NAME = "Testing";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator JoystickDirectionAffectHalalitDirection()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        float totalTime = 0.5f;
        float elapsedTime = 0f;
        float acceptedDelta = 1f;

        var halalitMovement = TestUtils.GetHalalitMovement();

        var randomTouchOnMovementJoystick = TestUtils.GetRandomTouch();

        // WHEN
        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            halalitMovement.TryMove(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y, deltaTime);
            yield return null;
        }

        // THEN
        AssertWrapper.AreEqual(
            halalitMovement.transform.rotation.eulerAngles.z, 
            Utils.AngleNormalizationBy360(Utils.Vector2ToDegrees(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y)),
            "Joystick vs Halalit Direction",
            seed,
            acceptedDelta);
    }

    [UnityTest]
    public IEnumerator JoystickSizeAffectHalalitSpeed()
    {
        // GIVEN
        var waitBetweenMovements = 2f;
        var totalTime = 0.5f;
        var smallVectorSize = 0.1f;

        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var halalitMovement = halalit.GetComponent<HalalitMovement>();
        var halalitRigidBody2D = halalit.GetComponent<Rigidbody2D>();

        var smallSizeTouchOnMovementJoystick = new Vector2(1 * smallVectorSize, 0);
        var largeSizeTouchOnMovementJoystick = new Vector2(1, 0);

        // WHEN

        yield return MoveHalalitForTime(halalitMovement, smallSizeTouchOnMovementJoystick, totalTime);
        var smallVelocity = halalitRigidBody2D.velocity.magnitude;

        yield return new WaitForSeconds(waitBetweenMovements);

        yield return MoveHalalitForTime(halalitMovement, largeSizeTouchOnMovementJoystick, totalTime);
        var largeVelocity = halalitRigidBody2D.velocity.magnitude;


        // THEN
        AssertWrapper.Less(smallVelocity, largeVelocity, "Speed Didn't rise as expected");
    }

    [UnityTest]
    public IEnumerator HalalitStopsMovingAfterTime()
    {
        // GIVEN
        var totalMovingTime = 0.3f;
        var elapsedTime = 0f;
        var waitUntilStopsMoving = 5f;

        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var halalitMovement = halalit.GetComponent<HalalitMovement>();
        var halalitRigidBody2D = halalit.GetComponent<Rigidbody2D>();

        // WHEN
        while (elapsedTime < totalMovingTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            halalitMovement.TryMove(1, 0, deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(waitUntilStopsMoving);

        // THEN
        AssertWrapper.AreEqual(halalitRigidBody2D.velocity.magnitude, 0, "Velocity is not 0");
    }

    private IEnumerator MoveHalalitForTime(HalalitMovement halalitMovement, Vector2 touchInput, float totalTime)
    {
        var elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;

            halalitMovement.TryMove(touchInput.x, touchInput.y, deltaTime);

            yield return null;
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}