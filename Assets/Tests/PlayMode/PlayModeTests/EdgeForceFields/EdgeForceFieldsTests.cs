using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

class EdgeForceFieldsTests
{
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    public struct TestArguments
    {
        public int XMovement;
        public int YMovement;
        public string ForceFieldName;

        public TestArguments(int xMovement, int yMovement, string forceFieldName)
        {
            XMovement = xMovement;
            YMovement = yMovement;
            ForceFieldName = forceFieldName;
        }
    }

    private static readonly TestArguments[] _fullEdgeForceFieldTestInputs = new TestArguments[] { 
        new TestArguments(0, 1, "TopWallForceField"),
        new TestArguments(0, -1, "BottomWallForceField"),
        new TestArguments(1, 0, "RightWallForceField"),
        new TestArguments(-1, 0, "LeftWallForceField")
    };

    private static readonly Tag[] _gameObjectsIgnoringEdgeForceField = new Tag[] { Tag.ENEMY, Tag.ITEM, Tag.VALUABLE };

    [UnityTest]
    public IEnumerator FullEdgeForceFieldHalalitTest([ValueSource(nameof(_fullEdgeForceFieldTestInputs))] TestArguments testArguments)
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();

        var halalit = GameObject.FindGameObjectWithTag(Tag.HALALIT.GetDescription());
        var halalitCollider = halalit.GetComponent<Collider2D>();
        var halalitMovement = halalit.GetComponent<HalalitMovement>();
        var halalitRigidBody = halalit.GetComponent<Rigidbody2D>();
        var forceFieldCollider = GameObject.Find(testArguments.ForceFieldName).GetComponent<Collider2D>();
        var lastHalalitVelocityBeforeForceField = 0f;
        var halalitInForceField = false;
        

        // WHEN
        while (!halalitInForceField)
        {
            lastHalalitVelocityBeforeForceField = halalitRigidBody.velocity.magnitude;
            halalitInForceField = TestUtils.AreCollidersTouch(halalitCollider, forceFieldCollider);

            halalitMovement.TryMove(testArguments.XMovement, testArguments.YMovement, Time.deltaTime); 
            yield return null;
        }

        for (int i = 0; i < 10; i++) // try move for 10 more frames
        {
            halalitMovement.TryMove(testArguments.XMovement, testArguments.YMovement, Time.deltaTime);
            yield return null;
        }

        // THEN
        AssertWrapper.IsTrue(lastHalalitVelocityBeforeForceField > halalitRigidBody.velocity.magnitude, "force field slower force didn't work", _currentSeed);

        // WHEN
        for (int i = 0; i < 100; i++) // stop move for 100 more frames
        {
            yield return null;
        }

        // THEN
        AssertWrapper.IsTrue(HalalitIsMovingBackward(halalitRigidBody.velocity, new Vector2(testArguments.XMovement, testArguments.YMovement)), "force field pusher force didn't work", _currentSeed);
    }

    [UnityTest]
    public IEnumerator GameObjectsIgnoringEdgeForceFieldTest(
        [ValueSource(nameof(_gameObjectsIgnoringEdgeForceField))] Tag gameObjectTag, 
        [ValueSource(nameof(_fullEdgeForceFieldTestInputs))] TestArguments testArguments)
    {   
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();

        switch (gameObjectTag)
        {
            case Tag.ENEMY:
                objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
                break;
            case Tag.ITEM:
                objectLoader.LoadItemInExternalSafeIsland();
                break;
            case Tag.VALUABLE:
                objectLoader.LoadValuableInExternalSafeIsland();
                break;
        }

        var gameObject = GameObject.FindGameObjectWithTag(gameObjectTag.GetDescription()); 
        var gameObjectCollider = gameObject.GetComponent<Collider2D>();
        var gameObjectRigidBody = gameObject.GetComponent<Rigidbody2D>();
        gameObjectRigidBody.drag = 0f;
        var forceFieldCollider = GameObject.Find(testArguments.ForceFieldName).GetComponent<Collider2D>();
        var lastGameObjectVelocityBeforeForceField = 0f;
        var gameObjectInForceField = false;

        gameObject.transform.position = new Vector2(3f, 3f);

        // WHEN
        gameObjectRigidBody.velocity = new Vector2(testArguments.XMovement, testArguments.YMovement) * 10;
        yield return null;

        while (!gameObjectInForceField)
        {
            lastGameObjectVelocityBeforeForceField = gameObjectRigidBody.velocity.magnitude;
            gameObjectInForceField = TestUtils.AreCollidersTouch(gameObjectCollider, forceFieldCollider);

            yield return null;
        }

        for (int i = 0; i < 5; i++) // try move for 5 more frames
        {
            yield return null;
        }

        // THEN
        AssertWrapper.AreEqual(lastGameObjectVelocityBeforeForceField, gameObjectRigidBody.velocity.magnitude, "force field slower force did work (not suppose to)", _currentSeed);
    }

    private bool HalalitIsMovingBackward(Vector2 currentVelocity, Vector2 movement)
    {
        if (movement.y > 0)
        {
            return currentVelocity.y < 0;
        }
        else if (movement.y < 0)
        {
            return currentVelocity.y > 0;
        }
        else if (movement.x > 0)
        {
            return currentVelocity.x < 0; 
        }
        else if (movement.x < 0)
        {
            return currentVelocity.x > 0; 
        }

        throw new Exception("illegal halalit movement");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}