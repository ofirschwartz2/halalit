using System.Collections.Generic;

// The first page of a leaderboard list (#312), copied from Unity's page by PlayerPlace.
// It holds no Unity Services types, so the tests can build a list without Unity's servers
public class LeaderboardPage
{
    public List<LeaderboardRow> Rows;
    // How many players are on the whole list, not only on this page
    public int Total;
}
