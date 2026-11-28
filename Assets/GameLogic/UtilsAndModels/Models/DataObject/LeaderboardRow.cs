// One row of a leaderboard list (#312), copied from Unity's entry by PlayerPlace.ToRow.
// It holds no Unity Services types, so the tests can build a list without Unity's servers
public class LeaderboardRow
{
    public string PlayerId;
    public string PlayerName;
    // As Unity's: 0 for first place
    public int Rank;
    public double Score;
}
