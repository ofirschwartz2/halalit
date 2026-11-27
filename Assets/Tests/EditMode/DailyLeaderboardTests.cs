using System;
using NUnit.Framework;

public class DailyLeaderboardTests
{

    [Test]
    public void SameDaySubmits()
    {
        var runDay = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var laterToday = new DateTime(2026, 11, 22, 23, 59, 0, DateTimeKind.Utc);

        var shouldSubmit = DailyLeaderboard.ShouldSubmitDaily(120, runDay, laterToday);

        AssertWrapper.IsTrue(shouldSubmit, "A daily score from today was not submitted");
    }

    [Test]
    public void NullScoreDoesNotSubmit()
    {
        var runDay = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var today = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);

        var shouldSubmit = DailyLeaderboard.ShouldSubmitDaily(null, runDay, today);

        AssertWrapper.IsFalse(shouldSubmit, "A missing daily score was submitted");
    }

    [Test]
    public void RunFromEarlierDayDoesNotSubmit()
    {
        var runDay = new DateTime(2026, 11, 22, 0, 0, 0, DateTimeKind.Utc);
        var oneMinutePastMidnight = new DateTime(2026, 11, 23, 0, 1, 0, DateTimeKind.Utc);

        var shouldSubmit = DailyLeaderboard.ShouldSubmitDaily(120, runDay, oneMinutePastMidnight.Date);

        AssertWrapper.IsFalse(shouldSubmit, "A run started on 22 Nov was submitted to the 23 Nov list");
    }

    [Test]
    public void DayIndexStaysBetweenTodayAndOldestArchivedDay()
    {
        AssertWrapper.AreEqual(0, DailyLeaderboard.ClampDayIndex(-1, 3), "Next day went past today");
        AssertWrapper.AreEqual(0, DailyLeaderboard.ClampDayIndex(1, 0), "Previous day opened with no archived days");
        AssertWrapper.AreEqual(2, DailyLeaderboard.ClampDayIndex(2, 3), "A day inside the archive was changed");
        AssertWrapper.AreEqual(3, DailyLeaderboard.ClampDayIndex(4, 3), "Previous day went past the oldest archived day");
        AssertWrapper.AreEqual(7, DailyLeaderboard.ClampDayIndex(8, 10), "Previous day went past 7 days");
    }

    [Test]
    public void ListTitles()
    {
        var archivedDayStart = new DateTime(2026, 11, 21, 0, 0, 0, DateTimeKind.Utc);

        AssertWrapper.AreEqual("Today", DailyLeaderboard.GetListTitle(false, 0, default), "Wrong title for today's list");
        AssertWrapper.AreEqual("21-11-26", DailyLeaderboard.GetListTitle(false, 1, archivedDayStart), "Wrong title for a past day");
        AssertWrapper.AreEqual("All time", DailyLeaderboard.GetListTitle(true, 1, archivedDayStart), "Wrong title for the all-time list");
    }

    [Test]
    public void PastDayTitleUsesIsraelDate()
    {
        // The daily list resets at midnight Israel time, which is 22:00 UTC the evening before in winter
        var archivedDayStart = new DateTime(2026, 11, 20, 22, 0, 0, DateTimeKind.Utc);

        AssertWrapper.AreEqual("21-11-26", DailyLeaderboard.GetListTitle(false, 1, archivedDayStart), "A past day was titled with its UTC date, not its Israel date");
    }

    [Test]
    public void PlaceLinesWithARank()
    {
        AssertWrapper.AreEqual("You: 12th of 40 so far", DailyLeaderboard.GetPlaceLine(false, 0, 11, 40), "Wrong place line for today's list");
        AssertWrapper.AreEqual("You: 3rd of 9", DailyLeaderboard.GetPlaceLine(false, 2, 2, 9), "Wrong place line for a past day");
        AssertWrapper.AreEqual("You: 1st of 25", DailyLeaderboard.GetPlaceLine(true, 0, 0, 25), "Wrong place line for the all-time list");
    }

    [Test]
    public void PlaceLinesWithoutARank()
    {
        AssertWrapper.AreEqual("You have not played today", DailyLeaderboard.GetPlaceLine(false, 0, null, 40), "Wrong line for today when you have not played");
        AssertWrapper.AreEqual("You did not play", DailyLeaderboard.GetPlaceLine(false, 2, null, 9), "Wrong line for a past day you did not play");
        AssertWrapper.AreEqual("You have no score yet", DailyLeaderboard.GetPlaceLine(true, 0, null, 25), "Wrong line for the all-time list with no score");
    }

    [Test]
    public void OnlyTodaysPlaceIsSoFar()
    {
        AssertWrapper.AreEqual("You: 1st of 1 so far", DailyLeaderboard.GetPlaceLine(false, 0, 0, 1), "Today's place did not say so far, though today's list is still open");
        AssertWrapper.AreEqual("You: 1st of 1", DailyLeaderboard.GetPlaceLine(false, 1, 0, 1), "Yesterday's place said so far, though yesterday's list is closed");
    }

    [Test]
    public void AllTimeNoScoreLineIgnoresTheDayItWasOpenedFrom()
    {
        // All time opened while a past day is shown keeps that day's index, as ShowAllTime passes _dayIndex
        AssertWrapper.AreEqual("You have no score yet", DailyLeaderboard.GetPlaceLine(true, 1, null, 25), "The all-time list showed a past day's line because it was opened from yesterday");
    }

    [Test]
    public void PlaceLinesOnAnEmptyDay()
    {
        AssertWrapper.AreEqual("You have not played today", DailyLeaderboard.GetPlaceLine(false, 0, null, 0), "Wrong line for today when nobody has played yet");
        AssertWrapper.AreEqual("You did not play", DailyLeaderboard.GetPlaceLine(false, 1, null, 0), "Wrong line for a past day nobody played");
    }

    [Test]
    public void PlayerRowKeepsItsPlaceOrGoesUnderTheRowsAfterAGap()
    {
        AssertWrapper.AreEqual(4, DailyLeaderboard.GetPlayerRowIndex(4, 10), "Your row moved although it was among the shown rows");
        AssertWrapper.AreEqual(11, DailyLeaderboard.GetPlayerRowIndex(-1, 10), "Your row was not put under the 10 shown rows after one empty row");
        AssertWrapper.AreEqual(21, DailyLeaderboard.GetPlayerRowIndex(-1, 20), "Your row did not follow a longer list: it was not put under 20 shown rows after one empty row");
    }
}
