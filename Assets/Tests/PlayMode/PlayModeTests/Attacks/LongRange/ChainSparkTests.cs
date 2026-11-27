using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class ChainSparkTests
{
    private int _currentSeed;
    private List<GameObject> _loadedEnemies;
    private const AttackName ATTACK_NAME = AttackName.CHAIN_SPARK;
    private const int TARGET_HEALTH = 100;
    // DEFAULT_ATTACK_STATS_2: Power 5 and Luck 0, so the bolt hits for 5 with no critical hit, and a jump hits for floor(5 * 0.7) = 3.
    private const int BOLT_HARM = 5;
    private const int JUMP_HARM = 3;
    private const float NEAR_DISTANCE = 2f;
    private const float FARTHER_DISTANCE = 2.5f;
    private const float BOLT_START_DISTANCE = 1.5f;
    private const float HALALIT_DISTANCE_BELOW_TARGETS = 4f;
    // Longer than the bolt's 0.5 second lifetime, so the bolt has hit or died by then.
    private const float SHOT_WAIT = 0.6f;
    private const float HIT_TIMEOUT = 1f;
    private const float LINE_LIFETIME_MARGIN = 0.1f;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator JumpsToNearestOnly()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(3);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        var first = enemies[0];
        var near = enemies[1];
        var farther = enemies[2];
        TestUtils.SetGameObjectPosition(first, anchor);
        TestUtils.SetGameObjectPosition(near, anchor + Vector2.right * NEAR_DISTANCE);
        TestUtils.SetGameObjectPosition(farther, anchor + Vector2.left * FARTHER_DISTANCE);
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);

        // WHEN
        yield return FireAt(anchor, Vector2.down);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - BOLT_HARM, GetHealth(first), "The Bolt Did Not Hit The First Enemy Once", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(near), "The Spark Did Not Jump To The Nearest Enemy For 70% Power", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(farther), "The Spark Hit An Enemy Out Of Reach Of The Last Target", _currentSeed);
    }

    [UnityTest]
    public IEnumerator StopsAfterMaxJumps()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(5);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        for (int i = 0; i < enemies.Count; i++)
        {
            TestUtils.SetGameObjectPosition(enemies[i], anchor + Vector2.right * NEAR_DISTANCE * i);
        }
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);
        var maxJumps = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<ChainSpark>().GetMaxJumps();

        // WHEN
        yield return FireAt(anchor, Vector2.down);

        // THEN
        AssertWrapper.AreEqual(3, maxJumps, "Chain Spark's Max Jumps Is Not 3 (Needs Ofir in Unity)", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - BOLT_HARM, GetHealth(enemies[0]), "The First Enemy Was Not Hit Only By The Bolt", _currentSeed);
        for (int i = 1; i <= 3; i++)
        {
            AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(enemies[i]), $"Jump {i} Did Not Hit Enemy {i} Once", _currentSeed);
        }
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(enemies[4]), "The Spark Jumped More Than 3 Times", _currentSeed);
    }

    [UnityTest]
    public IEnumerator TieGoesToTheLeft()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(3);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        var first = enemies[0];
        var right = enemies[1];
        var left = enemies[2];
        // Out of the ExternalSafeIsland parent, so no parent transform can round the positions and break the exact tie.
        foreach (var enemy in enemies)
        {
            enemy.transform.SetParent(null);
        }
        TestUtils.SetGameObjectPosition(first, anchor);
        TestUtils.SetGameObjectPosition(right, anchor + Vector2.right * NEAR_DISTANCE);
        TestUtils.SetGameObjectPosition(left, anchor + Vector2.left * NEAR_DISTANCE);
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);

        // WHEN
        yield return FireAt(anchor, Vector2.down);

        // THEN
        // From the left enemy, the right one is 4 away, beyond the 3 unit jump radius, so only the tie decides.
        AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(left), "On A Tie The Spark Did Not Jump To The Enemy On The Left", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH, GetHealth(right), "On A Tie The Spark Jumped To The Enemy On The Right", _currentSeed);
    }

    [UnityTest]
    public IEnumerator JumpsToAsteroid()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(1);
        var enemies = _loadedEnemies;
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var asteroid = objectLoader.LoadAsteroidInExternalSafeIsland(_currentSeed);
        yield return null;
        TestUtils.SetAsteroidsHealth(TARGET_HEALTH);
        Vector2 anchor = GetAnchor();
        TestUtils.SetGameObjectPosition(enemies[0], anchor);
        TestUtils.SetGameObjectPosition(asteroid, anchor + Vector2.right * NEAR_DISTANCE);
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);

        // WHEN
        // From the left, so the bolt cannot touch the asteroid first, whatever its size.
        yield return FireAt(anchor, Vector2.left);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - BOLT_HARM, GetHealth(enemies[0]), "The Bolt Did Not Hit The Enemy Once", _currentSeed);
        AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(asteroid), "The Spark Did Not Jump To The Asteroid", _currentSeed);
    }

    [UnityTest]
    public IEnumerator JumpGoesRoundMirrorShieldPlate()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(1);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        Vector2 shieldPosition = anchor + Vector2.right * NEAR_DISTANCE;
        var mirrorShield = TestUtils.LoadMirrorShield(shieldPosition, _currentSeed);
        yield return null;
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        TestUtils.SetGameObjectPosition(enemies[0], anchor);
        mirrorShield.transform.rotation = Quaternion.LookRotation(Vector3.forward, anchor - shieldPosition);
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);

        // WHEN
        yield return FireAt(anchor, Vector2.down);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(mirrorShield), "The Spark Did Not Go Round The Mirror Shield's Plate", _currentSeed);
    }

    [UnityTest]
    public IEnumerator JumpWakesHiddenMimic()
    {
        // GIVEN
        yield return SetUpChainSpark();
        yield return LoadEnemiesWithHealth(1);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        var mimic = TestUtils.LoadMimic(anchor + Vector2.right * NEAR_DISTANCE, _currentSeed);
        var disguisedWait = mimic.GetComponent<WaitAttackDisguisedWait>();
        yield return null;
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
        TestUtils.SetGameObjectPosition(enemies[0], anchor);
        Vector2 halalitPosition = anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS;
        yield return PlaceHalalit(halalitPosition);
        AssertWrapper.Greater(Vector2.Distance(halalitPosition, mimic.transform.position), disguisedWait.GetWakeRadius(), "Test Setup: The Halalit Is Inside The Mimic's Wake Radius", _currentSeed);
        AssertWrapper.IsTrue(disguisedWait.IsHidden(), "Test Setup: The Mimic Was Not Hidden Before The Shot", _currentSeed);

        // WHEN
        yield return FireAt(anchor, Vector2.down);

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - JUMP_HARM, GetHealth(mimic), "The Spark Did Not Jump To The Hidden Mimic", _currentSeed);
        AssertWrapper.IsFalse(disguisedWait.IsHidden(), "The Spark's Jump Did Not Wake The Hidden Mimic", _currentSeed);
    }

    [UnityTest]
    public IEnumerator LineShowsBriefly()
    {
        // GIVEN
        yield return SetUpChainSpark();
        var jumpLinePrefab = AttacksBank.GetAttackPrefab(ATTACK_NAME).GetComponent<ChainSpark>().GetJumpLinePrefab();
        AssertWrapper.IsNotNull(jumpLinePrefab, _currentSeed, "Chain Spark Has No Jump Line Prefab (Needs Ofir in Unity)");
        yield return LoadEnemiesWithHealth(2);
        var enemies = _loadedEnemies;
        Vector2 anchor = GetAnchor();
        TestUtils.SetGameObjectPosition(enemies[0], anchor);
        TestUtils.SetGameObjectPosition(enemies[1], anchor + Vector2.right * NEAR_DISTANCE);
        yield return PlaceHalalit(anchor + Vector2.down * HALALIT_DISTANCE_BELOW_TARGETS);

        // WHEN
        TestUtils.LoadChainSpark(anchor + Vector2.down * BOLT_START_DISTANCE, Vector2.up);
        var hitTimeout = Time.time + HIT_TIMEOUT;
        while (GetHealth(enemies[0]) == TARGET_HEALTH && Time.time < hitTimeout)
        {
            yield return new WaitForFixedUpdate();
        }
        var linesAfterHit = Object.FindObjectsOfType<ChainSparkLine>().Length;

        yield return new WaitForSeconds(jumpLinePrefab.GetLifeTime() + LINE_LIFETIME_MARGIN);
        yield return new WaitForFixedUpdate();
        yield return null;
        var linesAfterLifetime = Object.FindObjectsOfType<ChainSparkLine>().Length;

        // THEN
        AssertWrapper.AreEqual(TARGET_HEALTH - BOLT_HARM, GetHealth(enemies[0]), "Test Setup: The Bolt Did Not Hit The First Enemy", _currentSeed);
        AssertWrapper.AreEqual(1, linesAfterHit, "There Is Not Exactly One Spark Line Right After The Hit", _currentSeed);
        AssertWrapper.AreEqual(0, linesAfterLifetime, "The Spark Line Was Not Removed After Its Lifetime", _currentSeed);
    }

    private IEnumerator SetUpChainSpark()
    {
        yield return null;
        TestUtils.AssertAttackPrefabHas<ChainSpark>(ATTACK_NAME, _currentSeed);
        TestUtils.SetTestMode();
        TestUtils.SetUpShot(ATTACK_NAME, TestUtils.DEFAULT_ATTACK_STATS_2);
    }

    private IEnumerator LoadEnemiesWithHealth(int count)
    {
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        _loadedEnemies = objectLoader.LoadEnemiesInExternalSafeIsland(count, _currentSeed);
        yield return null;
        TestUtils.SetEnemiesHealth(TARGET_HEALTH);
    }

    // Whole numbers, so that distances along x come out exactly equal on both sides for the tie test.
    private Vector2 GetAnchor()
    {
        Vector2 center = TestUtils.GetInternalWorldBoxCollider2D().bounds.center;
        return new Vector2(Mathf.Round(center.x), Mathf.Round(center.y));
    }

    private IEnumerator PlaceHalalit(Vector2 position)
    {
        TestUtils.SetGameObjectPosition(TestUtils.GetHalalit(), position);
        yield return new WaitForFixedUpdate();
    }

    // Fires the bolt at the target from the given side, then waits until the bolt has hit or died.
    private IEnumerator FireAt(Vector2 target, Vector2 fromSide)
    {
        TestUtils.LoadChainSpark(target + fromSide * BOLT_START_DISTANCE, -fromSide);
        yield return new WaitForSeconds(SHOT_WAIT);
        yield return new WaitForFixedUpdate();
    }

    private int GetHealth(GameObject target)
    {
        return target.GetComponent<Health>().GetHealth();
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
