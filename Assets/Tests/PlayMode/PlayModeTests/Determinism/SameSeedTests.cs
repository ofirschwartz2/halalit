using Assets.Enums;
using Assets.Utils;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class SameSeedTests
{
    private const int NUMBER_OF_ENEMIES_TO_KILL = 10;
    private const string NO_ITEM = "No Item";

    private int _currentSeed;
    private bool _isTimeScaleChanged;

    // Drop recording: filled by the drop event handlers at the moment of each drop.
    private List<string> _recordedDrops;
    private int _currentKillIndex;
    private Transform _itemsParent;
    private HashSet<int> _knownItemIds;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator SameSeedSameEnemyOrder()
    {
        // GIVEN
        var firstRunEnemyOrder = GetEnemyOrder();

        // WHEN
        TestUtils.SetRandomSeed(_currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;

        var secondRunEnemyOrder = GetEnemyOrder();

        // THEN
        AssertWrapper.Greater(firstRunEnemyOrder.Count, 0, "No Enemies In The Enemy Bank", _currentSeed);
        AssertWrapper.AreEqual(firstRunEnemyOrder.Count, secondRunEnemyOrder.Count, "Different Number Of Enemies", _currentSeed);
        for (int i = 0; i < firstRunEnemyOrder.Count; i++)
        {
            AssertWrapper.AreEqual(firstRunEnemyOrder[i], secondRunEnemyOrder[i], "Different Enemy Order", _currentSeed);
        }
    }

    // Records the roll results at the moment of each drop (which valuable, which item, the push it got),
    // matched by kill order. Nothing depends on where a drop has rolled to afterwards.
    [UnityTest]
    public IEnumerator SameSeedSameDrops()
    {
        // GIVEN
        var firstRunDrops = new List<string>();
        yield return KillEnemiesAndRecordDrops(firstRunDrops);

        // WHEN
        TestTimeController.ResetTimeScale();
        TestUtils.SetRandomSeed(_currentSeed);
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        yield return null;

        var secondRunDrops = new List<string>();
        yield return KillEnemiesAndRecordDrops(secondRunDrops);

        // THEN
        AssertWrapper.Greater(firstRunDrops.Count, NUMBER_OF_ENEMIES_TO_KILL - 1, "Not Every Kill Rolled An Item Drop", _currentSeed);
        AssertWrapper.AreEqual(firstRunDrops.Count, secondRunDrops.Count, "Different Number Of Drops", _currentSeed);
        for (int i = 0; i < firstRunDrops.Count; i++)
        {
            AssertWrapper.AreEqual(firstRunDrops[i], secondRunDrops[i], "Different Drop", _currentSeed);
        }
    }

    private List<string> GetEnemyOrder()
    {
        var enemyBank = GameObject.FindGameObjectWithTag(Tag.ENEMIES_SPAWNER.GetDescription()).GetComponent<EnemyBank>();
        var enemyOrder = new List<string>();

        var spawnHoleEnemies = enemyBank.GetNextSpawnHoleEnemiesList();
        while (spawnHoleEnemies != null)
        {
            enemyOrder.Add("Group Size " + spawnHoleEnemies.Count);
            foreach (var enemyEntity in spawnHoleEnemies)
            {
                enemyOrder.Add(enemyEntity.Prefab.name + " " + enemyEntity.Seed);
            }

            spawnHoleEnemies = enemyBank.GetNextSpawnHoleEnemiesList();
        }

        return enemyOrder;
    }

    private IEnumerator KillEnemiesAndRecordDrops(List<string> drops)
    {
        TestUtils.SetTestMode();
        _isTimeScaleChanged = true;

        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        var loadedEnemies = objectLoader.LoadEnemiesInExternalSafeIsland(NUMBER_OF_ENEMIES_TO_KILL, _currentSeed);
        yield return null;

        // Inside the world, 2 apart, away from the Halalit and the edges, so nothing touches them.
        for (int i = 0; i < NUMBER_OF_ENEMIES_TO_KILL; i++)
        {
            TestUtils.SetGameObjectPosition(loadedEnemies[i], new Vector2((i * 2f) - 9f, 5f));
        }

        StartRecordingDrops(drops);
        yield return null; // the new drop event components take their handler snapshot in Start()
        yield return new WaitForFixedUpdate();

        for (int i = 0; i < NUMBER_OF_ENEMIES_TO_KILL; i++)
        {
            _currentKillIndex = i;
            var health = loadedEnemies[i].GetComponent<Health>();
            health.TryChangeHealth(-health.GetHealth());
        }
        yield return null;

        StopRecordingDrops();
        AssertWrapper.AreEqual(0, TestUtils.GetEnemies().Length, "Enemies Left Alive", _currentSeed);
    }

    private void StartRecordingDrops(List<string> drops)
    {
        _recordedDrops = drops;
        _itemsParent = TestUtils.GetItemDropper().transform;
        _knownItemIds = new HashSet<int>();
        foreach (Transform child in _itemsParent)
        {
            _knownItemIds.Add(child.gameObject.GetInstanceID());
        }

        // Added after ItemDropper and ValuableDropper, so these run right after the real drop.
        ItemDropEvent.NewItemDrop += RecordItemDrop;
        ValuableDropEvent.NewValuableDrop += RecordValuableDrop;

        // The drop events call a snapshot of their handlers taken in Start(), so take a new snapshot.
        TestUtils.GetEventSystem().AddComponent<ItemDropEvent>();
        TestUtils.GetEventSystem().AddComponent<ValuableDropEvent>();
    }

    private void StopRecordingDrops()
    {
        ItemDropEvent.NewItemDrop -= RecordItemDrop;
        ValuableDropEvent.NewValuableDrop -= RecordValuableDrop;
    }

    private void RecordItemDrop(object initiator, ItemDropEventArguments arguments)
    {
        string droppedItem = NO_ITEM;
        foreach (Transform child in _itemsParent)
        {
            if (_knownItemIds.Add(child.gameObject.GetInstanceID()))
            {
                droppedItem = child.name;
            }
        }

        _recordedDrops.Add("Kill " + _currentKillIndex + ": " + arguments.Dropper + " item " + droppedItem + ", force " + ToExactString(arguments.DropForce));
    }

    private void RecordValuableDrop(object initiator, ValuableDropEventArguments arguments)
    {
        _recordedDrops.Add("Kill " + _currentKillIndex + ": " + arguments.Dropper + " valuable " + arguments.ValuableName + ", force " + ToExactString(arguments.DropForce));
    }

    private static string ToExactString(Vector2 vector)
    {
        return "(" + vector.x.ToString("R", CultureInfo.InvariantCulture) + ", " + vector.y.ToString("R", CultureInfo.InvariantCulture) + ")";
    }

    [TearDown]
    public void TearDown()
    {
        StopRecordingDrops();
        TestUtils.DestroyAllGameObjects();
        if (_isTimeScaleChanged)
        {
            TestTimeController.ResetTimeScale();
            _isTimeScaleChanged = false;
        }
    }
}
