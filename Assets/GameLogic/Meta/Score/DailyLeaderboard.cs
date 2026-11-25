using System;
using Assets.Utils;

public static class DailyLeaderboard
{
    private const string TODAY_TITLE = "Today";
    private const string ALL_TIME_TITLE = "All time";

    // A run that crosses midnight Israel time belongs to the day it started, so it is not sent to the next day's list
    public static bool ShouldSubmitDaily(int? dailyScore, DateTime runDay, DateTime today)
    {
        return dailyScore != null && runDay.Date == today.Date;
    }

    // Day index 0 is today; 1 to the number of archived days are past days, newest first
    public static int ClampDayIndex(int dayIndex, int archivedDays)
    {
        int lastDayIndex = Math.Max(0, Math.Min(archivedDays, Constants.DAILY_LEADERBOARD_PAST_DAYS));
        return Math.Max(0, Math.Min(dayIndex, lastDayIndex));
    }

    public static string GetListTitle(bool isAllTime, int dayIndex, DateTime archivedDayStart)
    {
        if (isAllTime)
        {
            return ALL_TIME_TITLE;
        }

        if (dayIndex == 0)
        {
            return TODAY_TITLE;
        }

        // The list resets at midnight Israel time, which is the evening before in UTC
        return DailySeed.GetDateKey(DailySeed.GetIsraelDate(archivedDayStart));
    }
}
