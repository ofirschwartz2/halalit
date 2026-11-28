using System;
using Assets.Utils;
using NUnit.Framework;

public class DailyTrackStoreTests
{

    [Test]
    public void WinterKeyMatchesTheArchivedDay()
    {
        // The writer keys the track by the daily's Israel date. The reader will key it by the Israel date of the archived list's start,
        // which is 22:00 UTC the evening before in winter. If the two differ, friends' ships never load
        var writerDay = new DateTime(2026, 11, 21);
        var archivedDayStart = new DateTime(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

        var writerKey = DailyTrackStore.GetKey(writerDay);
        var readerKey = DailyTrackStore.GetKey(DailySeed.GetIsraelDate(archivedDayStart));

        AssertWrapper.AreEqual("DailyTrack_21-11-26", writerKey, "The track key is not DailyTrack_ and the daily's date");
        AssertWrapper.AreEqual(writerKey, readerKey, "In winter, the reader's key for a past day is not the writer's key");
    }

    [Test]
    public void SummerKeyMatchesTheArchivedDay()
    {
        // In summer the list starts at 21:00 UTC the evening before
        var writerDay = new DateTime(2026, 7, 15);
        var archivedDayStart = new DateTime(2026, 7, 14, 21, 0, 0, DateTimeKind.Utc);

        var writerKey = DailyTrackStore.GetKey(writerDay);
        var readerKey = DailyTrackStore.GetKey(DailySeed.GetIsraelDate(archivedDayStart));

        AssertWrapper.AreEqual("DailyTrack_15-07-26", writerKey, "The track key is not DailyTrack_ and the daily's date");
        AssertWrapper.AreEqual(writerKey, readerKey, "In summer, the reader's key for a past day is not the writer's key");
    }
}
