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
    private static readonly KeyValuePair<string, Tag>[] _scenesAndGameObjectTagsToPush = new KeyValuePair<string, Tag>[] { 
        new(TestUtils.TEST_SCENE_WITHOUT_TARGET_NAME, Tag.HALALIT),
        new(TestUtils.TEST_SCENE_WITH_ENEMY_NAME, Tag.ENEMY),
        new(TestUtils.TEST_SCENE_WITH_ASTEROID_NAME, Tag.ASTEROID),
        new(TestUtils.TEST_SCENE_WITH_ONE_ITEM_NAME, Tag.ITEM),
        new(TestUtils.TEST_SCENE_WITH_VALUABLES_NAME, Tag.VALUABLE)
    };
    private static readonly Vector2[] _givenMovement = new Vector2[] {Vector2.zero, new(10, 0)};

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed(); 
    }

    [UnityTest]
    public IEnumerator GameObjectBeingPushedNotPassingSpeedLimitTest(
        [ValueSource(nameof(_scenesAndGameObjectTagsToPush))] KeyValuePair<string, Tag> sceneAndGameObjectTagToPush,
        [ValueSource(nameof(_givenMovement))] Vector2 gameObjectGivenMovement)
    {
        // GIVEN
        SceneManager.LoadScene(sceneAndGameObjectTagToPush.Key);
        yield return null;

        var gameObjectToPush = GameObject.FindGameObjectWithTag(sceneAndGameObjectTagToPush.Value.GetDescription());
        var gameObjectToPushRigidBody = gameObjectToPush.GetComponent<Rigidbody2D>();
        var maxSpeedReached = 0f;

        SetGivenMovement(gameObjectToPushRigidBody, gameObjectGivenMovement);
        yield return null;

        // WHEN
        PushGameObject(gameObjectToPush, gameObjectToPushRigidBody);
        yield return null;

        if (gameObjectToPushRigidBody.isKinematic)
        {
            yield return null;
            maxSpeedReached = gameObjectToPush.GetComponent<KinematicMovement>().GetSpeed(); // No need to wait because kinematic speed does not change
        }
        else
        {
            while (gameObjectToPushRigidBody.velocity != Vector2.zero && gameObjectToPushRigidBody.velocity.magnitude > 5f)
            {
                yield return null;

                if (gameObjectToPushRigidBody.velocity.magnitude > maxSpeedReached)
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