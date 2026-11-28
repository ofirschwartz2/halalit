using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Keeps PlayerStats on quiet fakes for the whole test run (#307), so no test signs in for real or posts a score to the friends' leaderboard.
// The set-up fixture of each test assembly calls UseFakes before the first test and UseRealServices after the last one.
// A test that swaps in its own fakes puts these back with UseFakes in its [TearDown], never with PlayerStats.ResetServices
internal static class TestServices
{
    // Read only by the guard tests
    internal static int SignInCalls, SaveCalls, SubmitCalls;

    // Both calls run in one synchronous call, so no real call can run between them
    internal static void UseFakes()
    {
        // Also clears a running load and the save chain, so the next test starts on a fresh load
        PlayerStats.ResetServices();
        PlayerStats.SetServices(FakeSignIn, FakeSaveScores, FakeLoadScores, FakeSubmitScore, FakeSaveDailyTrack);
        SignInCalls = 0;
        SaveCalls = 0;
        SubmitCalls = 0;
    }

    // Only the two set-up fixtures call this, after the last test, so Play in the editor runs on the real services again
    internal static void UseRealServices()
    {
        PlayerStats.ResetServices();
    }

    #region Fakes
    // Quiet and instant: a test that dies and falls back to the menu keeps passing as before
    private static Task FakeSignIn()
    {
        SignInCalls++;
        return Task.CompletedTask;
    }

    private static Task FakeSaveScores(Dictionary<string, object> scores)
    {
        SaveCalls++;
        return Task.CompletedTask;
    }

    private static Task<Dictionary<string, int>> FakeLoadScores(HashSet<string> keys)
    {
        return Task.FromResult(new Dictionary<string, int>());
    }

    private static Task FakeSubmitScore(string leaderboardId, double score)
    {
        SubmitCalls++;
        return Task.CompletedTask;
    }

    private static Task FakeSaveDailyTrack(DateTime israelDay, string trackJson)
    {
        return Task.CompletedTask;
    }
    #endregion
}
