using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;

public class DailyTwistTests
{
    private const int SCENE_START_HEALTH = 30;
    private const float SCENE_SPAWN_INTERVAL = 5f;
    private const float SCENE_ASTEROID_MIN_SCALE = 1f;
    private const int GLASS_DAY_START_HEALTH = 12;
    private const float SWARM_SPAWN_INTERVAL = 3f;
    private const float BIG_ROCKS_ASTEROID_MIN_SCALE = 3f;
    private const float SPAWN_INTERVAL_DELTA = 0.001f;
    private const int DAYS_IN_A_ROW = 60;
    private const int DAYS_IN_A_YEAR = 365;
    private const int MOST_DAYS_IN_A_ROW_OF_ONE_TWIST = 2;
    private const string PREFABS_FOLDER = "Assets/Resources";
    private const string FOLLOWS_DAILY_TWIST_FIELD = "_followsDailyTwist";
    // One row per name but NONE, as EveryTwistHasTitleAndDescription checks
    private static readonly int TWIST_COUNT = Enum.GetValues(typeof(DailyTwistName)).Length - 1;

    [Test]
    public void SameDateSameTwist()
    {
        var utcMidnight = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var localLateEvening = new DateTime(2026, 11, 22, 23, 59, 0, DateTimeKind.Local);

        var utcTwist = DailyTwists.PickForDate(utcMidnight);
        var localTwist = DailyTwists.PickForDate(localLateEvening);

        AssertWrapper.IsTrue(utcTwist.Name == localTwist.Name, $"Same date gave different twists: {utcTwist.Name} and {localTwist.Name}");
    }

    [Test]
    public void SameDateSameTwistOnEveryCall()
    {
        // The pick keeps nothing between calls: a year of dates picked again, last date first, gives the same twists
        var firstDay = new DateTime(2026, 11, 21);
        var firstNames = new DailyTwistName[DAYS_IN_A_YEAR];

        for (var i = 0; i < DAYS_IN_A_YEAR; i++)
        {
            firstNames[i] = DailyTwists.PickForDate(firstDay.AddDays(i)).Name;
        }

        for (var i = DAYS_IN_A_YEAR - 1; i >= 0; i--)
        {
            var day = firstDay.AddDays(i);
            var name = DailyTwists.PickForDate(day).Name;

            AssertWrapper.IsTrue(name == firstNames[i], $"{day:yyyy-MM-dd} gave {firstNames[i]}, then {name}");
        }
    }

    [Test]
    public void PinnedTwists()
    {
        // Worked out outside Unity from the DailySeed hash and the block rule (#273). If one fails, find out why before changing it:
        // a changed twist list deals a different twist for every date, and phones on an older build would play another twist
        AssertPinnedTwist(new DateTime(2026, 11, 21), DailyTwistName.SWARM);
        AssertPinnedTwist(new DateTime(2026, 11, 22), DailyTwistName.BIG_ROCKS);
        AssertPinnedTwist(new DateTime(2026, 11, 23), DailyTwistName.SWARM);
        // 28 Nov to 6 Dec: three whole blocks, the dates of the play check on #286.
        // 30 Nov and 1 Dec are both Swarm because one block ends and the next starts there, so these also pin where the blocks start
        AssertPinnedTwist(new DateTime(2026, 11, 28), DailyTwistName.GLASS_DAY);
        AssertPinnedTwist(new DateTime(2026, 11, 29), DailyTwistName.BIG_ROCKS);
        AssertPinnedTwist(new DateTime(2026, 11, 30), DailyTwistName.SWARM);
        AssertPinnedTwist(new DateTime(2026, 12, 1), DailyTwistName.SWARM);
        AssertPinnedTwist(new DateTime(2026, 12, 2), DailyTwistName.GLASS_DAY);
        AssertPinnedTwist(new DateTime(2026, 12, 3), DailyTwistName.BIG_ROCKS);
        AssertPinnedTwist(new DateTime(2026, 12, 4), DailyTwistName.SWARM);
        AssertPinnedTwist(new DateTime(2026, 12, 5), DailyTwistName.GLASS_DAY);
        AssertPinnedTwist(new DateTime(2026, 12, 6), DailyTwistName.BIG_ROCKS);
    }

    [Test]
    public void EveryBlockHasEveryTwistOnce()
    {
        var blockFirstDay = GetFirstBlockDayFrom(new DateTime(2026, 11, 21));

        for (var block = 0; block < DAYS_IN_A_YEAR / TWIST_COUNT; block++)
        {
            var namesInBlock = new HashSet<DailyTwistName>();
            for (var i = 0; i < TWIST_COUNT; i++)
            {
                namesInBlock.Add(DailyTwists.PickForDate(blockFirstDay.AddDays(i)).Name);
            }

            AssertWrapper.IsFalse(namesInBlock.Contains(DailyTwistName.NONE), $"A day in the block from {blockFirstDay:yyyy-MM-dd} had no twist");
            AssertWrapper.AreEqual(TWIST_COUNT, namesInBlock.Count, $"The {TWIST_COUNT} days from {blockFirstDay:yyyy-MM-dd} did not give every twist once");

            blockFirstDay = blockFirstDay.AddDays(TWIST_COUNT);
        }
    }

    [Test]
    public void SameTwistAtMostTwoDaysInARow()
    {
        // The last day of one block and the first day of the next can share a twist, but a third day cannot
        var firstDay = new DateTime(2026, 11, 21);
        var previousName = DailyTwists.PickForDate(firstDay).Name;
        var daysInARow = 1;

        for (var i = 1; i < DAYS_IN_A_YEAR; i++)
        {
            var day = firstDay.AddDays(i);
            var name = DailyTwists.PickForDate(day).Name;
            daysInARow = name == previousName ? daysInARow + 1 : 1;

            AssertWrapper.IsTrue(daysInARow <= MOST_DAYS_IN_A_ROW_OF_ONE_TWIST, $"{name} came {daysInARow} days in a row, up to {day:yyyy-MM-dd}");
            previousName = name;
        }
    }

    [Test]
    public void EveryTwistComesUp()
    {
        var firstDay = new DateTime(2026, 11, 21);
        var pickedNames = new HashSet<DailyTwistName>();

        for (var i = 0; i < DAYS_IN_A_ROW; i++)
        {
            pickedNames.Add(DailyTwists.PickForDate(firstDay.AddDays(i)).Name);
        }

        AssertWrapper.IsFalse(pickedNames.Contains(DailyTwistName.NONE), $"A daily in {DAYS_IN_A_ROW} days in a row had no twist");
        foreach (DailyTwistName name in Enum.GetValues(typeof(DailyTwistName)))
        {
            if (name != DailyTwistName.NONE)
            {
                AssertWrapper.IsTrue(pickedNames.Contains(name), $"{name} did not come up in {DAYS_IN_A_ROW} days in a row");
            }
        }
    }

    [Test]
    public void NegativeBlockHashesPickATwist()
    {
        // About half the block hashes are below 0 as an int. Every day of those blocks must still pick a twist, without throwing.
        // So must the first and last dates a DateTime holds: day 0, whose block hash is 0, and the highest day number
        var firstDay = new DateTime(2026, 11, 21);
        var negativeHashDayCount = 0;

        for (var i = 0; i < DAYS_IN_A_ROW; i++)
        {
            var day = firstDay.AddDays(i);
            if (DailySeed.GetHash(GetBlockNumber(day)) < 0)
            {
                AssertDatePicksATwist(day);
                negativeHashDayCount++;
            }
        }

        AssertWrapper.IsTrue(negativeHashDayCount > 0, $"No block in {DAYS_IN_A_ROW} days had a negative hash, so nothing was checked");
        AssertDatePicksATwist(DateTime.MinValue);
        AssertDatePicksATwist(DateTime.MaxValue);
    }

    [Test]
    public void NoneKeepsSceneValues()
    {
        var none = DailyTwists.Get(DailyTwistName.NONE);

        AssertWrapper.IsTrue(none.IsNone(), $"Asking for no twist gave {none.Name}");
        AssertWrapper.AreEqual(SCENE_START_HEALTH, none.GetStartHealth(SCENE_START_HEALTH), "No twist changed the ship's start health");
        AssertWrapper.AreEqual(SCENE_SPAWN_INTERVAL, none.GetSpawnInterval(SCENE_SPAWN_INTERVAL), "No twist changed the spawn interval");
        AssertWrapper.AreEqual(SCENE_ASTEROID_MIN_SCALE, none.GetAsteroidMinScale(SCENE_ASTEROID_MIN_SCALE), "No twist changed the smallest asteroid scale");
    }

    [Test]
    public void KnobValues()
    {
        var glassDay = DailyTwists.Get(DailyTwistName.GLASS_DAY);
        var swarm = DailyTwists.Get(DailyTwistName.SWARM);
        var bigRocks = DailyTwists.Get(DailyTwistName.BIG_ROCKS);

        AssertWrapper.AreEqual(GLASS_DAY_START_HEALTH, glassDay.GetStartHealth(SCENE_START_HEALTH), "Glass Day does not start the ship with 12 health");
        AssertWrapper.AreEqual(SWARM_SPAWN_INTERVAL, swarm.GetSpawnInterval(SCENE_SPAWN_INTERVAL), "Swarm does not turn a 5 s spawn interval into 3 s", acceptedDelta: SPAWN_INTERVAL_DELTA);
        AssertWrapper.AreEqual(BIG_ROCKS_ASTEROID_MIN_SCALE, bigRocks.GetAsteroidMinScale(SCENE_ASTEROID_MIN_SCALE), "Big Rocks does not make the smallest asteroid scale 3");

        // Each twist changes one number and keeps the other two
        AssertWrapper.AreEqual(SCENE_SPAWN_INTERVAL, glassDay.GetSpawnInterval(SCENE_SPAWN_INTERVAL), "Glass Day changed the spawn interval");
        AssertWrapper.AreEqual(SCENE_ASTEROID_MIN_SCALE, glassDay.GetAsteroidMinScale(SCENE_ASTEROID_MIN_SCALE), "Glass Day changed the smallest asteroid scale");
        AssertWrapper.AreEqual(SCENE_START_HEALTH, swarm.GetStartHealth(SCENE_START_HEALTH), "Swarm changed the ship's start health");
        AssertWrapper.AreEqual(SCENE_ASTEROID_MIN_SCALE, swarm.GetAsteroidMinScale(SCENE_ASTEROID_MIN_SCALE), "Swarm changed the smallest asteroid scale");
        AssertWrapper.AreEqual(SCENE_START_HEALTH, bigRocks.GetStartHealth(SCENE_START_HEALTH), "Big Rocks changed the ship's start health");
        AssertWrapper.AreEqual(SCENE_SPAWN_INTERVAL, bigRocks.GetSpawnInterval(SCENE_SPAWN_INTERVAL), "Big Rocks changed the spawn interval");
    }

    [Test]
    public void EveryTwistHasTitleAndDescription()
    {
        foreach (DailyTwistName name in Enum.GetValues(typeof(DailyTwistName)))
        {
            if (name == DailyTwistName.NONE)
            {
                continue;
            }

            var twist = DailyTwists.Get(name);

            AssertWrapper.IsTrue(twist.Name == name, $"{name} has no row in the twist list");
            AssertWrapper.IsFalse(string.IsNullOrWhiteSpace(twist.Title), $"{name} has no title");
            AssertWrapper.IsFalse(string.IsNullOrWhiteSpace(twist.Description), $"{name} has no description");
            AssertWrapper.AreEqual(twist.Title + ": " + twist.Description, twist.GetDisplayLine(), $"{name} shows the wrong line on the menu");
        }
    }

    [Test]
    public void NoPrefabFollowsTheTwist()
    {
        // Needs Ofir in Unity, step 3: only the Halalit in the Playground ticks Follows Daily Twist.
        // On a prefab, Glass Day would give every enemy or asteroid made from it 12 health
        var checkedHealthCount = 0;

        foreach (var prefabGuid in UnityEditor.AssetDatabase.FindAssets("t:Prefab", new[] { PREFABS_FOLDER }))
        {
            var prefabPath = UnityEditor.AssetDatabase.GUIDToAssetPath(prefabGuid);
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(prefabPath);
            if (prefab == null)
            {
                continue;
            }

            foreach (var health in prefab.GetComponentsInChildren<Health>(true))
            {
                var followsDailyTwist = new UnityEditor.SerializedObject(health).FindProperty(FOLLOWS_DAILY_TWIST_FIELD);

                AssertWrapper.IsTrue(followsDailyTwist != null, $"Health has no {FOLLOWS_DAILY_TWIST_FIELD} field, update this test");
                AssertWrapper.IsFalse(followsDailyTwist.boolValue, $"{prefabPath} ticks Follows Daily Twist. Only the Halalit in the Playground should (Needs Ofir in Unity)");
                checkedHealthCount++;
            }
        }

        AssertWrapper.IsTrue(checkedHealthCount > 0, $"Found no Health on any prefab in {PREFABS_FOLDER}, so nothing was checked");
    }

    private void AssertPinnedTwist(DateTime day, DailyTwistName expectedName)
    {
        var twist = DailyTwists.PickForDate(day);

        AssertWrapper.IsTrue(twist.Name == expectedName, $"Twist for {day:yyyy-MM-dd} is {twist.Name}, expected {expectedName}");
    }

    private void AssertDatePicksATwist(DateTime day)
    {
        var twist = DailyTwists.PickForDate(day);

        AssertWrapper.IsFalse(twist.IsNone(), $"{day:yyyy-MM-dd} picked no twist");
    }

    // The days are dealt in blocks of one day per twist, counted from day 0, 1 Jan of year 1 (#273)
    private int GetBlockNumber(DateTime day)
    {
        return DailySeed.GetDayNumber(day) / TWIST_COUNT;
    }

    private DateTime GetFirstBlockDayFrom(DateTime day)
    {
        while (DailySeed.GetDayNumber(day) % TWIST_COUNT != 0)
        {
            day = day.AddDays(1);
        }

        return day;
    }
}
