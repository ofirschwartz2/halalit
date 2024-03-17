using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class PickupClawTests
{

    private const string SCENE_NAME = "Testing";

    [SetUp]
    public void SetUp()
    {
        SceneManager.LoadScene(SCENE_NAME);
    }
    
    /*
    [UnityTest]
    public IEnumerator ClawPressOnNothing()
    {
        
    }
    */
}