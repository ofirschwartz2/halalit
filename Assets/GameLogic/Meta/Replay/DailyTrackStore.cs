using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Utils;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

// Daily replay (#244): the only class that stores tracks. Each player writes one public key a day, so friends can read it.
// The day is the daily's own Israel date, the same as the private DailyScore_ key
public static class DailyTrackStore
{
    private const string KEY_START = "DailyTrack_";

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
}
