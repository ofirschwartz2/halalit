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
}
