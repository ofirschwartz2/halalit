using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class HalalitMovementTests
{

    private const string PlaygroundSceneName = "Playground";
    private const string HalalitTag = "Halalit";
    private const string MovementJoystickTag = "MovementJoystick";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(PlaygroundSceneName);
    }

    [UnityTest]
    public IEnumerator JoystickDirectionAffectHalalitDirection()
    {
        // GIVEN
        float totalTime = 1f;
        float elapsedTime = 0f;
        float acceptedDelta = 0.1f;

        GameObject halalit = GameObject.FindGameObjectWithTag(HalalitTag);
        HalalitMovement halalitMovement = halalit.GetComponent<HalalitMovement>();

        var randomTouchOnMovementJoystick = GetRandomTouchOnMovementJoystick();

        // WHEN
        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            halalitMovement.TryMove(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y, deltaTime);
            yield return null;
        }

        // THEN
        Assert.AreEqual(
            halalitMovement.transform.rotation.eulerAngles.z, 
            Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y)),
            acceptedDelta);
    }

    [UnityTest]
    public IEnumerator JoystickSizeAffectHalalitSpeed()
    {
        // GIVEN
        var waitBetweenMovements = 1f;
        var totalTime = 0.5f;

        var halalit = GameObject.FindGameObjectWithTag(HalalitTag);
        var halalitMovement = halalit.GetComponent<HalalitMovement>();
        var movementJoystickRadius = GameObject.FindGameObjectWithTag(MovementJoystickTag).transform.localScale.x / 2;

        var smallSizeTouchOnMovementJoystick = new Vector2(movementJoystickRadius / 10, 0);
        var largeSizeTouchOnMovementJoystick = new Vector2(movementJoystickRadius, 0);

        // WHEN

        yield return MoveHalalitForTime(halalitMovement, smallSizeTouchOnMovementJoystick, totalTime);
        var smallVelocity = halalit.GetComponent<Rigidbody2D>().velocity.magnitude;

        yield return new WaitForSeconds(waitBetweenMovements);

        yield return MoveHalalitForTime(halalitMovement, largeSizeTouchOnMovementJoystick, totalTime);
        var largeVelocity = halalit.GetComponent<Rigidbody2D>().velocity.magnitude;


        // THEN
        Assert.Less(smallVelocity, largeVelocity);
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

    private Vector2 GetRandomTouchOnMovementJoystick()
    {
        GameObject movementJoystick = GameObject.FindGameObjectWithTag(MovementJoystickTag);
        RectTransform movementJoystickRectTransform = movementJoystick.GetComponent<RectTransform>();
        return Utils.GetRandomVector2OnCircle(movementJoystickRectTransform.rect.width / 2);
    }
}