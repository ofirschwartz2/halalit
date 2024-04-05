using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class SpikesTests
{

    private const string SCENE_NAME = "Testing";
    private const string SCENE_WITH_TARGET_NAME = "TestingWithTarget";
    private const AttackName SHOT_NAME = AttackName.SPIKES;

    [SetUp]
    public void SetUp()
    {
        string testName = TestContext.CurrentContext.Test.MethodName;
        switch (testName) 
        {
            case FUNCTION_STABBING_WITH_TARGET_NAME:
                SceneManager.LoadScene(SCENE_WITH_TARGET_NAME);
                break;
            default:
                SceneManager.LoadScene(SCENE_NAME);
                break;
        }
    }

    [UnityTest]
    public IEnumerator Stabbing()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);
    }

    private const string FUNCTION_STABBING_WITH_TARGET_NAME = "StabbingWithTarget";
    [UnityTest]
    public IEnumerator StabbingWithTarget()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(1.5f);
        var originalTargetHealth = TestUtils.GetTargetHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetTargetNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, seed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (shot != null);

        // THEN

        //TODO: Check target's position
        var newTargetHealth = TestUtils.GetTargetHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", seed);
        AssertWrapper.IsTrue(true, "");
    }

    [UnityTest]
    public IEnumerator StabLifetime()
    {
        // GIVEN
        var seed = TestUtils.SetRandomSeed();

        TestUtils.SetUpShot(SHOT_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var spikes = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(spikes, seed);

        // GIVEN
        var oldSpikesPosition = spikes.transform.position + new Vector3(1,1,1);
        var newSpikesPosition = Vector3.zero;
        float supposedExtractionTime = spikes.GetComponent<Spikes>().GetExtractionTime();
        float supposedWaitingTime = spikes.GetComponent<Spikes>().GetWaitingTime();
        float supposedRetractionTime = spikes.GetComponent<Spikes>().GetRetractionTime();
        float supposedLifetime = supposedExtractionTime + supposedWaitingTime + supposedRetractionTime;
        float actualExtractionTime = 0, actualWaitingTime = 0, actualRetractionTime = 0;
        var acceptedDelta = 1f;

        SpikesState spikesState = SpikesState.EXTRACTING;
        // WHEN
        do
        {
            newSpikesPosition = spikes.transform.position;
            if (spikesState == SpikesState.EXTRACTING) 
            {
                if (newSpikesPosition != oldSpikesPosition)
                {
                    actualExtractionTime += Time.deltaTime;
                }
                else 
                {
                    spikesState = SpikesState.WAITING_OUT;
                }
            } 
            if (spikesState == SpikesState.WAITING_OUT)
            {
                if (newSpikesPosition == oldSpikesPosition)
                {
                    actualWaitingTime += Time.deltaTime;
                }
                else
                {
                    spikesState = SpikesState.RETRACTING;
                }
            } 
            if (spikesState == SpikesState.RETRACTING)
            {
                actualRetractionTime += Time.deltaTime;
            }
            oldSpikesPosition = newSpikesPosition;
            yield return null;

            spikes = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        } while (spikes != null);

        // THEN
        AssertWrapper.AreEqual(actualExtractionTime, supposedExtractionTime, "Wrong Extraction Time", seed, acceptedDelta);
        AssertWrapper.AreEqual(actualWaitingTime, supposedWaitingTime, "Wrong Waiting Time", seed, acceptedDelta);
        AssertWrapper.AreEqual(actualRetractionTime, supposedRetractionTime, "Wrong Retraction Time", seed, acceptedDelta);
        AssertWrapper.AreEqual(actualExtractionTime + actualWaitingTime + actualRetractionTime, supposedLifetime, "Wrong Lifetime", seed, acceptedDelta);

    }

}