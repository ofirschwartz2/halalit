// Daily replay (#244): one player in a finished day's replay. The name and score come from that day's list, the track from Cloud Save.
// It holds no Unity Services types, so the replay rules can be tested without Unity's servers
public class DailyReplayEntry
{
    // The place on the day's list, 1 = first. A player added from below the top 20 keeps their real place
    public int Place;
    public string PlayerName;
    // The score on the day's list
    public int Score;
    public bool IsPlayer;
    // Null when the player has no track, for example after playing on an older build. Such a player is listed in the results only
    public DailyTrack Track;
}
