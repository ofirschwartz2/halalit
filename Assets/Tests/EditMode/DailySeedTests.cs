using System;
using Assets.Utils;
using NUnit.Framework;

public class DailySeedTests
{

    [Test]
    public void SameDateSameSeed()
    {
        var utcMidnight = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var localLateEvening = new DateTime(2026, 11, 22, 23, 59, 0, DateTimeKind.Local);

        var utcSeed = DailySeed.GetSeed(utcMidnight);
        var localSeed = DailySeed.GetSeed(localLateEvening);

        AssertWrapper.IsTrue(utcSeed == localSeed, $"Same date gave different seeds: {utcSeed} and {localSeed}");
    }

    [Test]
    public void NextDateDifferentSeed()
    {
        var today = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var tomorrow = new DateTime(2026, 11, 23, 0, 0, 0, DateTimeKind.Utc);

        var todaySeed = DailySeed.GetSeed(today);
        var tomorrowSeed = DailySeed.GetSeed(tomorrow);

        AssertWrapper.IsTrue(todaySeed != tomorrowSeed, $"Two dates gave the same seed: {todaySeed}");
    }

    [Test]
    public void PinnedSeeds()
    {
        AssertPinnedSeed(new DateTime(2026, 11, 22), 114539602);
        AssertPinnedSeed(new DateTime(2026, 11, 23), -1359240184);
        AssertPinnedSeed(new DateTime(2025, 1, 1), -665835459);
    }

    private void AssertPinnedSeed(DateTime day, int expectedSeed)
    {
        var seed = DailySeed.GetSeed(day);

        AssertWrapper.IsTrue(seed == expectedSeed, $"Seed for {day:yyyy-MM-dd} is {seed}, expected {expectedSeed}");
    }
}
