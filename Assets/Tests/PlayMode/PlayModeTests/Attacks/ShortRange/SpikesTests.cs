using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class SpikesTests
{
    private int _currentSeed;
    private const AttackName ATTACK_NAME = AttackName.SPIKES;
    private const Tag ATTACK_TAG = Tag.SHOT;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator Stabbing()
    {
        // GIVEN
        TestUtils.SetTestMode();

        TestUtils.SetUpShot(AttackName.SPIKES);
        var weaponAttack = TestUtils.GetWeaponAttack();
        var randomTouchOnAttackJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());
        
        // WHEN
        weaponAttack.HumbleFixedUpdate(randomTouchOnAttackJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(Tag.SHOT.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);
    }

    [UnityTest]
    public IEnumerator StabbingWithTarget()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        TestUtils.SetRandomEnemyPosition(1.5f);
        var originalTargetHealth = TestUtils.GetEnemyHealth();
        yield return null;
        var targetClosestPosition = TestUtils.GetEnemyNearestPositionToHalalit();
        var touchOnJoystick = TestUtils.GetTouchOverAttackTriggetTowardsPosition(targetClosestPosition, weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(shot, _currentSeed);

        // GIVEN
        Vector2 lastShotPosition;

        // WHEN
        do
        {
            lastShotPosition = shot.transform.position;
            yield return null;

            shot = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (shot != null);

        // THEN

        //TODO: Check target's position
        var newTargetHealth = TestUtils.GetEnemyHealth();
        AssertWrapper.Greater(originalTargetHealth, newTargetHealth, "Target Health Didn't drop", _currentSeed);
        AssertWrapper.IsTrue(true, "");
    }

    [UnityTest]
    public IEnumerator StabLifetime()
    {
        // GIVEN
        TestUtils.SetTestMode();

        TestUtils.SetUpShot(ATTACK_NAME);
        var weaponMovement = TestUtils.GetWeaponMovement();
        var weaponAttack = TestUtils.GetWeaponAttack();
        var touchOnJoystick = TestUtils.GetRandomTouchOverAttackTrigger(weaponAttack.GetAttackJoystickEdge());

        // WHEN
        weaponMovement.TryChangeWeaponPosition(touchOnJoystick);
        yield return null;

        weaponAttack.HumbleFixedUpdate(touchOnJoystick);
        yield return null;

        // THEN
        var spikes = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        AssertWrapper.IsNotNull(spikes, _currentSeed);

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

            spikes = GameObject.FindGameObjectWithTag(ATTACK_TAG.GetDescription());
        } while (spikes != null);

        // THEN
        AssertWrapper.AreEqual(actualExtractionTime, supposedExtractionTime, "Wrong Extraction Time", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(actualWaitingTime, supposedWaitingTime, "Wrong Waiting Time", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(actualRetractionTime, supposedRetractionTime, "Wrong Retraction Time", _currentSeed, acceptedDelta);
        AssertWrapper.AreEqual(actualExtractionTime + actualWaitingTime + actualRetractionTime, supposedLifetime, "Wrong Lifetime", _currentSeed, acceptedDelta);

    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
}