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
    private const string PREFABS_FOLDER = "Assets/Resources";
    private const string FOLLOWS_DAILY_TWIST_FIELD = "_followsDailyTwist";

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
    public void PinnedTwists()
    {
        // Worked out from the DailySeed hash. If one fails, find out why before changing it:
        // a changed twist list deals a different twist for every date, and phones on an older build would play another twist
        AssertPinnedTwist(new DateTime(2026, 11, 21), DailyTwistName.BIG_ROCKS);
        AssertPinnedTwist(new DateTime(2026, 11, 22), DailyTwistName.SWARM);
        AssertPinnedTwist(new DateTime(2026, 11, 23), DailyTwistName.GLASS_DAY);
        AssertPinnedTwist(new DateTime(2026, 11, 28), DailyTwistName.BIG_ROCKS);
    }

    [Test]
    public void PickForDateMatchesPickForSeed()
    {
        var firstDay = new DateTime(2026, 11, 21);

        for (var i = 0; i < DAYS_IN_A_ROW; i++)
        {
            var day = firstDay.AddDays(i);
            var twistForDate = DailyTwists.PickForDate(day);
            var twistForSeed = DailyTwists.PickForSeed(DailySeed.GetSeed(day));

            AssertWrapper.IsTrue(twistForDate.Name == twistForSeed.Name, $"On {day:yyyy-MM-dd} the date picked {twistForDate.Name} and its seed picked {twistForSeed.Name}");
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
    public void NegativeSeedsPickATwist()
    {
        AssertSeedPicksATwist(int.MinValue);
        AssertSeedPicksATwist(-1);
        AssertSeedPicksATwist(0);
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

    private void AssertSeedPicksATwist(int seed)
    {
        var twist = DailyTwists.PickForSeed(seed);

        AssertWrapper.IsFalse(twist.IsNone(), $"Seed {seed} picked no twist");
    }
}
