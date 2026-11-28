using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Utils;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using LoadOptions = Unity.Services.CloudSave.Models.Data.Player.LoadOptions;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

// Daily replay (#244): the only class that stores tracks. Each player writes one public key a day, so friends can read it.
// The day is the daily's own Israel date, the same as the private DailyScore_ key
public static class DailyTrackStore
{
    private const string KEY_START = "DailyTrack_";
    // A replay shows 20 ships at most
    private const int REPLAY_PLAYER_LIMIT = 20;

    public static string GetKey(DateTime israelDay)
    {
        return KEY_START + DailySeed.GetDateKey(israelDay);
    }

    // Public data: any player can read it, only this player can write it
    public static async Task SaveAsync(DateTime israelDay, string trackJson)
    {
        var data = new Dictionary<string, object>
        {
            { GetKey(israelDay), trackJson }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data, new SaveOptions(new PublicWriteAccessClassOptions()));
    }

    // A finished day's players, in list order. The day's archived list stays the one record of who played, with names and scores;
    // each listed player's track is then read by player id. You are added under the top 20 if you played and are below them
    public static async Task<List<DailyReplayEntry>> LoadDayAsync(string versionId, DateTime israelDay)
    {
        var page = await LeaderboardsService.Instance.GetVersionScoresAsync(Constants.DAILY_LEADERBOARD_ID, versionId, new GetVersionScoresOptions { Limit = REPLAY_PLAYER_LIMIT });
        LeaderboardEntry playerEntry = await PlayerPlace.GetVersionPlayerEntryAsync(Constants.DAILY_LEADERBOARD_ID, versionId);

        var listEntries = new List<LeaderboardEntry>(page.Results);
        string playerId = playerEntry == null ? null : playerEntry.PlayerId;

        if (playerEntry != null && !listEntries.Exists(entry => entry.PlayerId == playerId))
        {
            listEntries.Add(playerEntry);
        }

        string key = GetKey(israelDay);
        var trackLoads = new Task<DailyTrack>[listEntries.Count];

        for (int i = 0; i < listEntries.Count; i++)
        {
            trackLoads[i] = LoadTrackOrNullAsync(listEntries[i].PlayerId, key);
        }

        DailyTrack[] tracks = await Task.WhenAll(trackLoads);
        var entries = new List<DailyReplayEntry>(listEntries.Count);

        for (int i = 0; i < listEntries.Count; i++)
        {
            entries.Add(new DailyReplayEntry
            {
                // Unity's Rank starts at 0, as in LeaderBoard.AddRow, so both screens show the same place
                Place = listEntries[i].Rank + 1,
                PlayerName = listEntries[i].PlayerName,
                // The game submits whole scores; the list keeps them as doubles
                Score = Convert.ToInt32(listEntries[i].Score),
                IsPlayer = playerId != null && listEntries[i].PlayerId == playerId,
                Track = tracks[i]
            });
        }

        return entries;
    }

    // Null when the player has no track for the day, or it could not be read. That player is shown without a ship, and the replay goes on
    private static async Task<DailyTrack> LoadTrackOrNullAsync(string playerId, string key)
    {
        try
        {
            var items = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key }, new LoadOptions(new PublicReadAccessClassOptions(playerId)));

            if (!items.TryGetValue(key, out var item))
            {
                return null;
            }

            return DailyTrackCodec.FromJson(item.Value.GetAs<string>());
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"DailyTrackStore: could not read a track, so that player has no ship. {exception.Message}");
            return null;
        }
    }
}
