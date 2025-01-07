using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

class SpeedLimitTests
{
    private Vector2 PUSH_VECTOR = new(500, 0);
    private int _currentSeed;
    private static readonly Tag[] _gameObjectTagsToPush = new Tag[] { 
        Tag.HALALIT,
        Tag.ENEMY,
        Tag.ASTEROID,
        Tag.ITEM,
        Tag.VALUABLE
    };

    private static readonly Vector2[] _givenMovement = new Vector2[] {Vector2.zero, new(10, 0)};
    private static readonly Vector2 TEST_POSITION = new(2, 2);

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator GameObjectBeingPushedNotPassingSpeedLimitTest (
        [ValueSource(nameof(_gameObjectTagsToPush))] Tag gameObjectTagToPush,
        [ValueSource(nameof(_givenMovement))] Vector2 gameObjectGivenMovement)
    {
        // GIVEN
        TestUtils.SetTestMode();

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();

        switch (gameObjectTagToPush)
        {
            case Tag.ENEMY:
                objectLoader.LoadEnemyInExternalSafeIsland();
                yield return null;
                TestUtils.SetEnemyPosition(TEST_POSITION);
                break;
            case Tag.ASTEROID:
                objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
                yield return null;
                TestUtils.SetAsteroidPosition(TEST_POSITION);
                break;
            case Tag.ITEM:
                objectLoader.LoadItemInExternalSafeIsland();
                yield return null;
                TestUtils.SetItemPosition(TEST_POSITION);
                break;
            case Tag.VALUABLE:
                objectLoader.LoadValuableInExternalSafeIsland();
                yield return null;
                TestUtils.SetValuablePosition(TEST_POSITION);
                break;
            case Tag.HALALIT:
                // Halalit is already in the scene
                break;
        }
        yield return null;

        var gameObjectToPush = GameObject.FindGameObjectWithTag(gameObjectTagToPush.GetDescription());
        var gameObjectToPushRigidBody = gameObjectToPush.GetComponent<Rigidbody2D>();
        var maxSpeedReached = 0f;

        SetGivenMovement(gameObjectToPushRigidBody, gameObjectGivenMovement);
        yield return null;

        // WHEN
        PushGameObject(gameObjectToPush, gameObjectToPushRigidBody);
        yield return new WaitForSeconds(0.1f); // Give time for the speed to be limited

        if (gameObjectToPushRigidBody.isKinematic)
        {
            yield return null;
            maxSpeedReached = gameObjectToPush.GetComponent<KinematicMovement>().GetSpeed(); // No need to wait because kinematic speed does not change
        }
        else
        {
            while (gameObjectToPushRigidBody != null && gameObjectToPushRigidBody.velocity.magnitude > 5f)
            {
                yield return null;

                if (gameObjectToPushRigidBody != null && gameObjectToPushRigidBody.velocity.magnitude > maxSpeedReached)
                {
                    maxSpeedReached = gameObjectToPushRigidBody.velocity.magnitude;
                }
            }
        }

        // THEN
        AssertWrapper.IsTrue(SpeedLimiter.IsAllowedSpeed(maxSpeedReached), "Reached speed is heigher than max speed allowed", _currentSeed);
    }

    private void SetGivenMovement(Rigidbody2D gameObjectToPushRigidBody, Vector2 gameObjectGivenMovement)
    {
        if (gameObjectToPushRigidBody.isKinematic)
        {
            KinematicMovement kinematicMovement = gameObjectToPushRigidBody.gameObject.GetComponent<KinematicMovement>();
            kinematicMovement.SetDirection(gameObjectGivenMovement.normalized);
            kinematicMovement.SetSpeed(gameObjectGivenMovement.magnitude);
        }
        else
        {
            gameObjectToPushRigidBody.velocity = gameObjectGivenMovement;
        }
    }

    private void PushGameObject(GameObject gameObjectToPush, Rigidbody2D gameObjectToPushRigidBody)
    {
        if (gameObjectToPushRigidBody.isKinematic)
        {
            KinematicMovement kinematicMovement = gameObjectToPush.GetComponent<KinematicMovement>();
            kinematicMovement.SetDirection(PUSH_VECTOR.normalized);
            kinematicMovement.SetSpeed(PUSH_VECTOR.magnitude);
        }
        else
        {
            gameObjectToPushRigidBody.AddForceAtPosition(PUSH_VECTOR, gameObjectToPush.transform.position, ForceMode2D.Impulse);
        }
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}