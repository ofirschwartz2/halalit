using System;
using System.Collections.Generic;
using System.Text;

// Daily replay (#244): who is crowned, who is still flying, and the words on the replay screen. Pure, so EditMode tests cover it
public static class DailyReplayRules
{
    private const string PLACE_END = ". ";
    private const string COLUMN_GAP = "  ";
    private const string NO_REPLAY_TEXT = "no replay";
    private const string WINNER_TEXT = "Winner: ";
    private const string LINE_END = "\n";

    // -1 when nobody played
    public static int GetWinnerIndex(IList<DailyReplayEntry> entries)
    {
        List<int> ranking = GetRanking(entries);
        return ranking.Count == 0 ? -1 : ranking[0];
    }

    // Entry indexes in crown order, first place first
    public static List<int> GetRanking(IList<DailyReplayEntry> entries)
    {
        var ranking = new List<int>(entries.Count);

        for (int i = 0; i < entries.Count; i++)
        {
            ranking.Add(i);
        }

        // The earlier entry stays ahead on a full tie. List.Sort is not stable, so the index decides last
        ranking.Sort((first, second) =>
        {
            int order = CompareForCrown(entries[first], entries[second]);
            return order != 0 ? order : first.CompareTo(second);
        });

        return ranking;
    }

    // The crown rule, the one place to change it. Ofir's provisional rule (#244, still an open question): the highest score wins,
    // a tie goes to the longer survival. For "last ship standing", compare survival first.
    // Negative when the first entry is ahead
    private static int CompareForCrown(DailyReplayEntry first, DailyReplayEntry second)
    {
        if (first.Score != second.Score)
        {
            return second.Score.CompareTo(first.Score);
        }

        return GetSurvivalSeconds(second).CompareTo(GetSurvivalSeconds(first));
    }

    // 0 for a player without a track
    public static float GetSurvivalSeconds(DailyReplayEntry entry)
    {
        return entry.Track == null ? 0f : entry.Track.SurvivalSeconds;
    }

    // The replay ends when the last ship is out
    public static float GetEndSeconds(IList<DailyReplayEntry> entries)
    {
        float endSeconds = 0f;

        foreach (DailyReplayEntry entry in entries)
        {
            endSeconds = Math.Max(endSeconds, GetSurvivalSeconds(entry));
        }

        return endSeconds;
    }

    // "m:ss", rounded down to the second
    public static string FormatClock(float seconds)
    {
        int wholeSeconds = (int)Math.Floor(Math.Max(0f, seconds));
        return (wholeSeconds / 60) + ":" + (wholeSeconds % 60).ToString("00");
    }

    // Under a ship once it is out: its final score and how long it lasted
    public static string GetOutText(int finalScore, float survivalSeconds)
    {
        return finalScore + COLUMN_GAP + FormatClock(survivalSeconds);
    }

    // Empty when nobody played
    public static string GetWinnerLine(IList<DailyReplayEntry> entries)
    {
        int winnerIndex = GetWinnerIndex(entries);
        return winnerIndex == -1 ? string.Empty : WINNER_TEXT + entries[winnerIndex].PlayerName;
    }

    // One line per player in crown order: the place on the day's list, name, score and survival time
    public static string GetResultsText(IList<DailyReplayEntry> entries)
    {
        List<int> ranking = GetRanking(entries);
        var text = new StringBuilder();

        for (int place = 0; place < ranking.Count; place++)
        {
            DailyReplayEntry entry = entries[ranking[place]];

            if (place > 0)
            {
                text.Append(LINE_END);
            }

            text.Append(entry.Place).Append(PLACE_END).Append(entry.PlayerName)
                .Append(COLUMN_GAP).Append(entry.Score)
                .Append(COLUMN_GAP).Append(entry.Track == null ? NO_REPLAY_TEXT : FormatClock(entry.Track.SurvivalSeconds));
        }

        return text.ToString();
    }

    #region Predicates
    // A ship flies until its player died. A player without a track never flies
    public static bool IsAlive(DailyReplayEntry entry, float time)
    {
        return entry.Track != null && time < entry.Track.SurvivalSeconds;
    }
    #endregion
}
