using System;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Exceptions;
using Unity.Services.Leaderboards.Models;

public static class PlayerPlace
{
    private const string OF_TEXT = " of ";
    private const string SO_FAR_TEXT = " so far";
    private const string FIRST_SUFFIX = "st";
    private const string SECOND_SUFFIX = "nd";
    private const string THIRD_SUFFIX = "rd";
    private const string OTHER_SUFFIX = "th";

    // Null when you have no score on the list
    public static Task<LeaderboardEntry> GetPlayerEntryAsync(string leaderboardId)
    {
        return GetEntryOrNullAsync(() => LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId));
    }

    // Null when you have no score on that past version of the list.
    // Unity returns a past version's entry as a LeaderboardVersionEntry, so it is copied into a LeaderboardEntry, the type of the list's rows
    public static Task<LeaderboardEntry> GetVersionPlayerEntryAsync(string leaderboardId, string versionId)
    {
        return GetEntryOrNullAsync(async () =>
        {
            var versionEntry = await LeaderboardsService.Instance.GetVersionPlayerScoreAsync(leaderboardId, versionId);

            if (versionEntry == null)
            {
                return null;
            }

            return new LeaderboardEntry(versionEntry.PlayerId, versionEntry.PlayerName, versionEntry.Rank, versionEntry.Score);
        });
    }

    // How many players are on the list. Only the page's Total is read, so one row is enough
    public static async Task<int> GetPlayerCountAsync(string leaderboardId)
    {
        var page = await LeaderboardsService.Instance.GetScoresAsync(leaderboardId, new GetScoresOptions { Limit = 1 });
        return page.Total;
    }

    // Unity's Rank starts at 0 for first place. Two calls can disagree for a moment, so the count is never below the place
    public static string GetPlaceText(int rank, int playerCount, bool isSoFar)
    {
        int place = rank + 1;
        int count = Math.Max(playerCount, place);
        string placeText = GetOrdinal(place) + OF_TEXT + count;

        return isSoFar ? placeText + SO_FAR_TEXT : placeText;
    }

    // 1st, 2nd, 3rd, 4th; 11, 12 and 13 (and 111, 112, 113) end in th
    public static string GetOrdinal(int place)
    {
        int lastTwoDigits = place % 100;

        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return place + OTHER_SUFFIX;
        }

        switch (place % 10)
        {
            case 1:
                return place + FIRST_SUFFIX;
            case 2:
                return place + SECOND_SUFFIX;
            case 3:
                return place + THIRD_SUFFIX;
            default:
                return place + OTHER_SUFFIX;
        }
    }

    // EntryNotFound and NotFound both mean you have no score on the list, which is not an error here. Other errors go on to the screen
    private static async Task<LeaderboardEntry> GetEntryOrNullAsync(Func<Task<LeaderboardEntry>> getEntryAsync)
    {
        try
        {
            return await getEntryAsync();
        }
        catch (LeaderboardsException exception)
        {
            if (exception.Reason == LeaderboardsExceptionReason.EntryNotFound || exception.Reason == LeaderboardsExceptionReason.NotFound)
            {
                return null;
            }

            throw;
        }
    }
}
