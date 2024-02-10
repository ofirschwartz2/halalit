using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class JoystickDirectionTest
{

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Playground");
    }

    [UnityTest]
    public IEnumerator JoystickDirection()
    {
        float totalTime = 1f;
        float elapsedTime = 0f;
        GameObject halalit = GameObject.FindGameObjectWithTag("Halalit");
        HalalitMovement halalitMovement = halalit.GetComponent<HalalitMovement>();

        var randomTouchOnMovementJoystick = GetRandomTouchOnMovementJoystick();

        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            halalitMovement.TryMove(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y, deltaTime);
            yield return null;
        }

        Assert.AreEqual(
            halalitMovement.transform.rotation.eulerAngles.z, 
            Utils.AngleNormalizationBy360(Utils.Vector2ToDegree(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y)),
            0.1);
    }

    private Vector2 GetRandomTouchOnMovementJoystick()
    {
        GameObject movementJoystick = GameObject.FindGameObjectWithTag("MovementJoystick");
        RectTransform movementJoystickRectTransform = movementJoystick.GetComponent<RectTransform>();
        return Utils.GetRandomVector2OnCircle(movementJoystickRectTransform.rect.width / 2);
    }
}