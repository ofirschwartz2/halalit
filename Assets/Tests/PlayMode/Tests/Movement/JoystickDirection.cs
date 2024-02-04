using NUnit.Framework;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class JoystickDirectionTest
{

    [DllImport("AllAssembly.dll", EntryPoint = "Assets.Utils.GetRandomVector2OnCircle")]
    public static extern Vector2 GetRandomVector2OnCircle(float radius);

    [DllImport("AllAssembly.dll", EntryPoint = "Assets.GameLogic.Player.Halalit.HalalitMovement")]
    public static extern void TryMove(float joystickHorizontal, float joystickVertical, float deltaTime);


    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene("Playground");
    }

    [UnityTest]
    public IEnumerator JoystickDirection()
    {
        float totalTime = 3f;
        float elapsedTime = 0f;
        
        var randomTouchOnMovementJoystick = GetRandomTouchOnMovementJoystick();

        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;
            elapsedTime += deltaTime;
            TryMove(randomTouchOnMovementJoystick.x, randomTouchOnMovementJoystick.y, deltaTime);

            yield return null;
        }

        Assert.IsTrue(true);
    }

    private Vector2 GetRandomTouchOnMovementJoystick()
    {
        GameObject movementJoystick = GameObject.FindGameObjectWithTag("MovementJoystick");
        RectTransform movementJoystickRectTransform = movementJoystick.GetComponent<RectTransform>();
        return GetRandomVector2OnCircle(movementJoystickRectTransform.rect.width / 2);
    }
}
