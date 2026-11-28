using System;
using Assets.Utils;

public static class DailyLeaderboard
{
    private const string TODAY_TITLE = "Today";
    private const string ALL_TIME_TITLE = "All time";
    private const string PLACE_LINE_START = "You: ";
    private const string NO_SCORE_YET_LINE = "You have no score yet";
    private const string NOT_PLAYED_TODAY_LINE = "You have not played today";
    private const string DID_NOT_PLAY_LINE = "You did not play";

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

    // Your place on the list on screen. Today's list is still open, so its place is "so far"
    public static string GetPlaceLine(bool isAllTime, int dayIndex, int? rank, int playerCount)
    {
        if (rank != null)
        {
            return PLACE_LINE_START + PlayerPlace.GetPlaceText(rank.Value, playerCount, !isAllTime && dayIndex == 0);
        }

        if (isAllTime)
        {
            return NO_SCORE_YET_LINE;
        }

        if (dayIndex == 0)
        {
            return NOT_PLAYED_TODAY_LINE;
        }

        return DID_NOT_PLAY_LINE;
    }

    // Daily replay (#244): Watch is for finished days only. Today's list is still open, and all time is not one day
    public static bool CanWatch(bool isAllTime, int dayIndex)
    {
        return !isAllTime && dayIndex > 0;
    }

    // When you are below the shown rows, your row goes under them, after one empty row as a gap
    public static int GetRowIndexUnderShownRows(int shownRows)
    {
        return shownRows + 1;
    }
}
