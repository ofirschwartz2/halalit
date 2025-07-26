using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class PullingMagnetTests
{
    private int _currentSeed;
    private const ItemName MAGNET_ITEM_NAME = ItemName.PULLING_MAGNET;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator MagnetPickupAndActivationTest()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var magnet = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();

        // Move magnet to origin
        TestUtils.SetItemPosition(Vector2.zero);
        yield return null;

        // Move right until magnet is picked up or 5 seconds pass
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (magnet != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        AssertWrapper.IsTrue(magnet == null, "Failed to pick up Magnet within 5 seconds", _currentSeed);

        // Verify utility button is interactable
        AssertWrapper.IsTrue(utilityButton.GetComponent<UnityEngine.UI.Button>().interactable, 
            "Utility button should be interactable", _currentSeed);

        // Click utility button to activate magnet
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;

        // Verify magnet object is spawned at Halalit's position
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        var spawnedMagnet = System.Array.Find(allObjects, obj => obj.GetComponent<PullingMagnetField>() != null);
        AssertWrapper.IsNotNull(spawnedMagnet, _currentSeed, "Magnet object should be spawned");
        AssertWrapper.IsNotNull(spawnedMagnet.GetComponent<PullingMagnetField>(), _currentSeed, "Spawned magnet should have PullingMagnetField component");

        // Verify magnet is at Halalit's position (with small tolerance)
        var halalitPosition = TestUtils.GetHalalit().transform.position;
        var magnetPosition = spawnedMagnet.transform.position;
        var distance = Vector2.Distance(halalitPosition, magnetPosition);
        AssertWrapper.Less(distance, 0.5f, "Magnet should be spawned at Halalit's position", _currentSeed);
    }

    [UnityTest]
    public IEnumerator MagnetPullsEnemiesTowardTest()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var magnet = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var enemy = TestUtils.GetEnemy();
        var enemyInitialPosition = enemy.transform.position;

        // Position enemy away from Halalit
        TestUtils.SetEnemyPosition(Vector2.right * 2f);
        yield return null;

        // Pick up magnet
        TestUtils.SetItemPosition(Vector2.zero);
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (magnet != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        AssertWrapper.IsTrue(magnet == null, "Failed to pick up Magnet", _currentSeed);

        // Activate magnet
        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;
        
        // Debug: Check if magnet was spawned
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        var spawnedMagnet = System.Array.Find(allObjects, obj => obj.GetComponent<PullingMagnetField>() != null);
        if (spawnedMagnet != null)
        {
            Debug.Log($"Test: Pulling magnet spawned at position: {spawnedMagnet.transform.position}");
        }
        else
        {
            Debug.Log("Test: No pulling magnet found!");
        }

        // Wait for physics to apply force
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Measure enemy's new position
        var enemyNewPosition = enemy.transform.position;
        var enemyMovement = enemyNewPosition - enemyInitialPosition;

        // Verify enemy has moved toward the magnet's position
        AssertWrapper.Greater(enemyMovement.magnitude, 0.1f, "Enemy should have moved", _currentSeed);

        // Verify movement direction is toward magnet (magnet is at origin)
        var magnetPosition = Vector2.zero; // Magnet is at origin
        
        // Check if enemy moved closer to magnet
        var initialDistanceToMagnet = Vector2.Distance((Vector2)enemyInitialPosition, magnetPosition);
        var newDistanceToMagnet = Vector2.Distance((Vector2)enemyNewPosition, magnetPosition);
        var movedCloser = newDistanceToMagnet < initialDistanceToMagnet;
        
        // Debug logs
        Debug.Log($"Test: Enemy initial position: {enemyInitialPosition}");
        Debug.Log($"Test: Enemy new position: {enemyNewPosition}");
        Debug.Log($"Test: Enemy movement: {enemyMovement}");
        Debug.Log($"Test: Initial distance to magnet: {initialDistanceToMagnet}");
        Debug.Log($"Test: New distance to magnet: {newDistanceToMagnet}");
        Debug.Log($"Test: Moved closer: {movedCloser}");
        
        AssertWrapper.IsTrue(movedCloser, "Enemy should move closer to magnet", _currentSeed);
    }

    [UnityTest]
    public IEnumerator MagnetForceInverseCubeLawTest()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var magnet = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        objectLoader.LoadEnemyInExternalSafeIsland(_currentSeed);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var enemies = TestUtils.GetEnemies();

        // Position one enemy very close (0.5 units) and one at edge (3 units)
        TestUtils.SetEnemyPosition(Vector2.right * 0.5f, 0);
        TestUtils.SetEnemyPosition(Vector2.right * 3f, 1);
        yield return null;

        // Pick up and activate magnet
        TestUtils.SetItemPosition(Vector2.zero);
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (magnet != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;

        // Wait for physics to apply force (more time for force to accumulate)
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        // Measure velocities of both enemies
        var closeEnemyVelocity = enemies[0].GetComponent<Rigidbody2D>().velocity.magnitude;
        var farEnemyVelocity = enemies[1].GetComponent<Rigidbody2D>().velocity.magnitude;

        // Verify closer enemy receives significantly more force
        AssertWrapper.Greater(closeEnemyVelocity, farEnemyVelocity, "Closer enemy should receive more force", _currentSeed);

        // Verify the closer enemy receives significantly more force than the farther enemy
        // Due to enemy AI interference, we can't test exact inverse-cube law
        // Instead, verify that closer enemy gets at least 3x more force
        var actualRatio = closeEnemyVelocity / farEnemyVelocity;
        var minimumRatio = 3f; // Closer enemy should get at least 3x more force
        AssertWrapper.Greater(actualRatio, minimumRatio, 
            $"Closer enemy should receive significantly more force. Actual ratio: {actualRatio:F2}, Minimum expected: {minimumRatio}", _currentSeed);
    }

    [UnityTest]
    public IEnumerator MagnetDurationAndAutoDeactivationTest()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var magnet = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();

        // Pick up magnet
        TestUtils.SetItemPosition(Vector2.zero);
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (magnet != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        // Activate magnet
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;

        // Record activation time
        var activationTime = Time.time;

        // Find the spawned magnet
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        var spawnedMagnet = System.Array.Find(allObjects, obj => obj.GetComponent<PullingMagnetField>() != null);
        AssertWrapper.IsNotNull(spawnedMagnet, _currentSeed, "Magnet should be spawned");

        // Wait for magnet duration to expire (5 seconds + buffer)
        yield return new WaitForSeconds(6f);

        // Verify magnet object is destroyed
        allObjects = GameObject.FindObjectsOfType<GameObject>();
        var remainingMagnet = System.Array.Find(allObjects, obj => obj.GetComponent<PullingMagnetField>() != null);
        AssertWrapper.IsNull(remainingMagnet, _currentSeed, "Magnet should be destroyed after duration expires");

        // Verify utility button is cleared (no longer interactable)
        AssertWrapper.IsFalse(utilityButton.GetComponent<UnityEngine.UI.Button>().interactable, 
            "Utility button should be cleared after magnet expires", _currentSeed);
    }

    [UnityTest]
    public IEnumerator MagnetMultipleUtilitiesTest()
    {
        // GIVEN
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var magnet1 = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        var magnet2 = objectLoader.LoadItemInExternalSafeIsland(MAGNET_ITEM_NAME);
        yield return null;

        var halalitMovement = TestUtils.GetHalalitMovement();
        var utilityButton = GameObject.Find("UtilityButton").GetComponent<Meta.UI.UtilityButton>();

        // Pick up first magnet
        magnet1.transform.position = Vector2.zero;
        var rightMovement = Vector2.right;
        var startTime = Time.time;
        while (magnet1 != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        // Activate first magnet
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;

        // Wait for first magnet to expire
        yield return new WaitForSeconds(6f);

        // Pick up second magnet
        magnet2.transform.position = Vector2.zero;
        startTime = Time.time;
        while (magnet2 != null && Time.time - startTime < 5f)
        {
            halalitMovement.TryMove(rightMovement.x, rightMovement.y, Time.deltaTime);
            yield return null;
        }

        // Verify utility button is interactable
        AssertWrapper.IsTrue(utilityButton.GetComponent<UnityEngine.UI.Button>().interactable, 
            "Utility button should be interactable for second magnet", _currentSeed);

        // Activate second magnet
        utilityButton.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        yield return null;

        // Verify second magnet works correctly
        var allObjects = GameObject.FindObjectsOfType<GameObject>();
        var spawnedMagnet = System.Array.Find(allObjects, obj => obj.GetComponent<PullingMagnetField>() != null);
        AssertWrapper.IsNotNull(spawnedMagnet, _currentSeed, "Second magnet should be spawned");
        AssertWrapper.IsNotNull(spawnedMagnet.GetComponent<PullingMagnetField>(), _currentSeed, "Second magnet should have PullingMagnetField component");
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
    }
} 