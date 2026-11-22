using System;
using System.Globalization;
using System.Threading;
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

    [Test]
    public void DateKeyIgnoresDeviceCulture()
    {
        var day = new DateTime(2026, 11, 22);
        var originalCulture = Thread.CurrentThread.CurrentCulture;

        try
        {
            // Thai culture uses the Buddhist calendar, so a culture-dependent key would read "22-11-69"
            Thread.CurrentThread.CurrentCulture = new CultureInfo("th-TH");
            var thaiKey = DailySeed.GetDateKey(day);

            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            var invariantKey = DailySeed.GetDateKey(day);

            AssertWrapper.AreEqual("22-11-26", thaiKey, "Date key changed with the device culture");
            AssertWrapper.AreEqual("22-11-26", invariantKey, "Date key has the wrong format");
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
        }
    }

    private void AssertPinnedSeed(DateTime day, int expectedSeed)
    {
        var seed = DailySeed.GetSeed(day);

        AssertWrapper.IsTrue(seed == expectedSeed, $"Seed for {day:yyyy-MM-dd} is {seed}, expected {expectedSeed}");
    }
}
