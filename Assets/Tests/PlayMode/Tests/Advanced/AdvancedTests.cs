using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class AdvancedTests
{

    private const string SCENE_NAME = "Testing";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }
    /*
    [UnityTest]
    public IEnumerator AllInWorld()
    {

        float totalTime = 100f;
        float checkInterval = 10f;
        float timeSpeedMultiplier = 20f;

        Time.timeScale = timeSpeedMultiplier;

        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            float deltaTime = Time.deltaTime;

            elapsedTime += deltaTime;

            bool allInWorld = CheckAllInWorld();

            if (!allInWorld)
            {
                Assert.Fail("Not all GameObjects are inside InternalWorld at time: " + elapsedTime);
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }
    */
    private bool CheckAllInWorld()
    {
        List<GameObject> allGameObjects = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        allGameObjects.Add(GameObject.Find("Halalit"));

        foreach (GameObject gameObject in allGameObjects)
        {
            if (!gameObject.GetComponent<Collider2D>().IsTouching(GameObject.Find("InternalWorld").GetComponent<Collider2D>()))
            {
                return false;
            }
        }

        return true;
    }


}
