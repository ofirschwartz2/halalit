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

    [Test]
    public void WinterDayChangesAtIsraelMidnight()
    {
        AssertIsraelDate(new DateTime(2026, 11, 24, 21, 59, 59, DateTimeKind.Utc), new DateTime(2026, 11, 24));
        AssertIsraelDate(new DateTime(2026, 11, 24, 22, 0, 0, DateTimeKind.Utc), new DateTime(2026, 11, 25));
    }

    [Test]
    public void SummerDayChangesAtIsraelMidnight()
    {
        AssertIsraelDate(new DateTime(2026, 7, 14, 20, 59, 59, DateTimeKind.Utc), new DateTime(2026, 7, 14));
        AssertIsraelDate(new DateTime(2026, 7, 14, 21, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 15));
    }

    [Test]
    public void SpringClockChangeKeepsIsraelMidnight()
    {
        // Summer time starts on Friday 27 Mar 2026 and Friday 26 Mar 2027 at 02:00 Israel time
        AssertIsraelDate(new DateTime(2026, 3, 26, 21, 59, 0, DateTimeKind.Utc), new DateTime(2026, 3, 26));
        AssertIsraelDate(new DateTime(2026, 3, 26, 22, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 27));
        AssertIsraelDate(new DateTime(2026, 3, 27, 20, 59, 0, DateTimeKind.Utc), new DateTime(2026, 3, 27));
        AssertIsraelDate(new DateTime(2026, 3, 27, 21, 0, 0, DateTimeKind.Utc), new DateTime(2026, 3, 28));

        AssertIsraelDate(new DateTime(2027, 3, 25, 21, 59, 0, DateTimeKind.Utc), new DateTime(2027, 3, 25));
        AssertIsraelDate(new DateTime(2027, 3, 25, 22, 0, 0, DateTimeKind.Utc), new DateTime(2027, 3, 26));
        AssertIsraelDate(new DateTime(2027, 3, 26, 20, 59, 0, DateTimeKind.Utc), new DateTime(2027, 3, 26));
        AssertIsraelDate(new DateTime(2027, 3, 26, 21, 0, 0, DateTimeKind.Utc), new DateTime(2027, 3, 27));
    }

    [Test]
    public void AutumnClockChangeKeepsIsraelMidnight()
    {
        // Summer time ends on Sunday 25 Oct 2026 and Sunday 31 Oct 2027 at 02:00 Israel time
        AssertIsraelDate(new DateTime(2026, 10, 24, 21, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 25));
        AssertIsraelDate(new DateTime(2026, 10, 25, 21, 59, 0, DateTimeKind.Utc), new DateTime(2026, 10, 25));
        AssertIsraelDate(new DateTime(2026, 10, 25, 22, 0, 0, DateTimeKind.Utc), new DateTime(2026, 10, 26));

        AssertIsraelDate(new DateTime(2027, 10, 30, 21, 0, 0, DateTimeKind.Utc), new DateTime(2027, 10, 31));
        AssertIsraelDate(new DateTime(2027, 10, 31, 21, 59, 0, DateTimeKind.Utc), new DateTime(2027, 10, 31));
        AssertIsraelDate(new DateTime(2027, 10, 31, 22, 0, 0, DateTimeKind.Utc), new DateTime(2027, 11, 1));
    }

    [Test]
    public void LocalTimeGivesSameIsraelDateAsUtc()
    {
        var utcInstant = new DateTime(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);
        var localInstant = utcInstant.ToLocalTime();

        var utcDate = DailySeed.GetIsraelDate(utcInstant);
        var localDate = DailySeed.GetIsraelDate(localInstant);

        AssertWrapper.IsTrue(localDate == utcDate, $"Device time gave {localDate:yyyy-MM-dd}, UTC gave {utcDate:yyyy-MM-dd}");
    }

    private void AssertPinnedSeed(DateTime day, int expectedSeed)
    {
        var seed = DailySeed.GetSeed(day);

        AssertWrapper.IsTrue(seed == expectedSeed, $"Seed for {day:yyyy-MM-dd} is {seed}, expected {expectedSeed}");
    }

    private void AssertIsraelDate(DateTime utcInstant, DateTime expectedDate)
    {
        var israelDate = DailySeed.GetIsraelDate(utcInstant);

        AssertWrapper.IsTrue(israelDate == expectedDate, $"Israel date for {utcInstant:yyyy-MM-dd HH:mm:ss} UTC is {israelDate:yyyy-MM-dd}, expected {expectedDate:yyyy-MM-dd}");
    }
}
