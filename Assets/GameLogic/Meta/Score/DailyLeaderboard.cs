using System;

public static class DailyLeaderboard
{
    // A run that crosses midnight UTC belongs to the day it started, so it is not sent to the next day's list
    public static bool ShouldSubmitDaily(int? dailyScore, DateTime runDay, DateTime today)
    {
        return dailyScore != null && runDay.Date == today.Date;
    }
}
