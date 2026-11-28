using System;

// One finished day of the daily list (#312), copied from Unity's archived version by PlayerPlace.
// It holds no Unity Services types, so the tests can build a past day without Unity's servers
public class LeaderboardDay
{
    // The archived version's id, which Unity needs to read that day's list
    public string VersionId;
    // As Unity's: when the day's list started, in UTC
    public DateTime Start;
}
