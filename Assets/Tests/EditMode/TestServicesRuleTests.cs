using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// No test may sign in for real or post a score to the friends' leaderboard (#307). EditModeServicesSetUp puts PlayerStats, LeaderBoard and DailyReplay on fakes
// before the first test, and only TestServices may put the real services back
public class TestServicesRuleTests
{
    private const string TESTS_FOLDER = "Tests";
    private const string GAME_CODE_FOLDER = "GameLogic";
    private const string TEST_SERVICES_SCRIPT = nameof(TestServices) + ".cs";
    private const string PLAY_MODE_SET_UP_SCRIPT = nameof(PlayModeServicesSetUp) + ".cs";
    private const string EDIT_MODE_SET_UP_SCRIPT = nameof(EditModeServicesSetUp) + ".cs";
    private const string RESET_SERVICES_CALL = "ResetServices(";
    private const string USE_REAL_SERVICES_CALL = "UseRealServices(";

    // Calls that reach Unity's servers with no PlayerStats, LeaderBoard or DailyReplay fake in between: the SDK itself, and the game methods that call it directly.
    // The scans skip comments and string literals, so this list does not find itself
    private static readonly string[] DIRECT_SERVICE_CALLS =
    {
        "UnityServices.InitializeAsync",
        "AuthenticationService.Instance",
        "CloudSaveService.Instance",
        "LeaderboardsService.Instance",
        "Authentication.Initialize",
        "Authentication.UpdatePlayerNameAsync",
        "PlayerPlace.GetPlayerEntryAsync",
        "PlayerPlace.GetVersionPlayerEntryAsync",
        "PlayerPlace.GetPlayerCountAsync",
        "PlayerPlace.GetPageAsync",
        "PlayerPlace.GetPastDaysAsync",
        "PlayerPlace.GetPastDayPageAsync",
        "DailyTrackStore.SaveAsync",
        "DailyTrackStore.LoadDayAsync",
    };

    // The proof that the fixture ran. The fake sign-in alone is not: PlayerStatsTests' tear-down also switches to the fakes,
    // and NUnit may run it first. Only the fixture sets this flag. Only reads
    [Test]
    public void SetUpFixturePutTheRunOnTheFakes()
    {
        AssertWrapper.IsTrue(TestServices.IsRunOnFakes, "EditModeServicesSetUp did not run before this test. Do not run the full suite: a test could sign in or post a score for real. Check that the fixture has no namespace line");
    }

    // Only reads, so it is safe to run even when the set-up fixture did not run
    [Test]
    public void EveryTestStartsOnTheFakeSignIn()
    {
        AssertWrapper.IsFalse(PlayerStats.IsSignInReal(), "PlayerStats is on the real sign-in, so the set-up fixture did not run or a test put the real services back. Do not run the full suite: a test could sign in or post a score for real");
    }

    // Only reads, so it is safe to run even when the set-up fixture did not run
    [Test]
    public void EveryTestStartsOnTheFakeLeaderboard()
    {
        AssertWrapper.IsFalse(LeaderBoard.IsListLoadReal(), "LeaderBoard is on the real list load, so the set-up fixture did not run or a test put the real services back. Do not run the full suite: a test could ask Unity's servers for the friends' leaderboard");
    }

    [Test]
    public void OnlyTestServicesPutsTheRealServicesBack()
    {
        // A test that puts the real services back in its [TearDown] leaves every later test on them, so its deaths could post for real
        var scriptsThatCallIt = FindTestScriptsThatCall(RESET_SERVICES_CALL, TEST_SERVICES_SCRIPT);

        AssertWrapper.IsTrue(scriptsThatCallIt.Count == 0, $"These scripts call {RESET_SERVICES_CALL}) on PlayerStats, LeaderBoard or DailyReplay, which puts the real services back for every later test: {string.Join(", ", scriptsThatCallIt)}. Call TestServices.UseFakes() instead");
    }

    [Test]
    public void OnlyTheSetUpFixturesEndTheRunOnFakes()
    {
        // UseRealServices is for after the last test. A test that calls it leaves every later test on the real services
        var scriptsThatCallIt = FindTestScriptsThatCall(USE_REAL_SERVICES_CALL, TEST_SERVICES_SCRIPT, PLAY_MODE_SET_UP_SCRIPT, EDIT_MODE_SET_UP_SCRIPT);

        AssertWrapper.IsTrue(scriptsThatCallIt.Count == 0, $"These scripts call TestServices.{USE_REAL_SERVICES_CALL}), which puts the real services back for every later test: {string.Join(", ", scriptsThatCallIt)}. Only the two set-up fixtures may call it");
    }

    [Test]
    public void NoTestCallsUnityServicesItself()
    {
        // The fakes cover only PlayerStats, LeaderBoard and DailyReplay. A test that calls the SDK, or a game method that calls it directly, goes past them
        var gameCode = ReadCodeWithoutCommentsOrLiterals(GAME_CODE_FOLDER, nameof(Authentication) + ".cs");
        var testCode = ReadCodeWithoutCommentsOrLiterals(TESTS_FOLDER, TEST_SERVICES_SCRIPT);
        var callsInTests = new List<string>();

        foreach (var call in DIRECT_SERVICE_CALLS)
        {
            // A call the game no longer makes means a name changed, and the scan would miss the new one
            AssertWrapper.IsTrue(IsInAnyScript(gameCode, call), $"No game script calls {call} any more, so this check looks for a stale name. Update the list with the new name");

            foreach (var scriptPathAndCode in testCode)
            {
                if (scriptPathAndCode.Value.Contains(call))
                {
                    callsInTests.Add($"{scriptPathAndCode.Key} calls {call}");
                }
            }
        }

        AssertWrapper.IsTrue(callsInTests.Count == 0, $"Tests call Unity's services directly, past the fakes: {string.Join("; ", callsInTests)}. Go through PlayerStats, LeaderBoard or DailyReplay on the fakes, or ask the architect for a swappable service");
    }

    // The scripts that call the method, apart from the allowed ones. Fails if the call is not found in the first allowed script,
    // so a renamed method or a moved folder cannot make the check pass with nothing found
    private static List<string> FindTestScriptsThatCall(string call, params string[] allowedScripts)
    {
        var testCode = ReadCodeWithoutCommentsOrLiterals(TESTS_FOLDER, allowedScripts[0]);
        var scriptsThatCallIt = new List<string>();
        var doesFirstAllowedScriptCallIt = false;

        foreach (var scriptPathAndCode in testCode)
        {
            var scriptName = Path.GetFileName(scriptPathAndCode.Key);
            var isAllowed = System.Array.IndexOf(allowedScripts, scriptName) >= 0;
            var doesCallIt = scriptPathAndCode.Value.Contains(call);

            doesFirstAllowedScriptCallIt = doesFirstAllowedScriptCallIt || (scriptName == allowedScripts[0] && doesCallIt);

            if (doesCallIt && !isAllowed)
            {
                scriptsThatCallIt.Add(scriptPathAndCode.Key);
            }
        }

        AssertWrapper.IsTrue(doesFirstAllowedScriptCallIt, $"{allowedScripts[0]} no longer calls {call}), so this check looks for a stale name. Update the check");

        return scriptsThatCallIt;
    }

    // Every script under Assets/<folder>, by path, with its comments and string literals blanked out. A call inside an interpolated string's braces is missed.
    // Fails if the folder or the expected script is missing, so a scan that looked in the wrong place cannot pass
    private static Dictionary<string, string> ReadCodeWithoutCommentsOrLiterals(string folder, string expectedScript)
    {
        var folderPath = Path.Combine(Application.dataPath, folder);
        AssertWrapper.IsTrue(Directory.Exists(folderPath), $"{folderPath} does not exist, so nothing was checked");

        var codeByScriptPath = new Dictionary<string, string>();
        var didFindExpectedScript = false;

        foreach (var scriptPath in Directory.GetFiles(folderPath, "*.cs", SearchOption.AllDirectories))
        {
            codeByScriptPath[scriptPath] = AuthenticationTests.COMMENT_OR_LITERAL.Replace(File.ReadAllText(scriptPath), " ");
            didFindExpectedScript = didFindExpectedScript || Path.GetFileName(scriptPath) == expectedScript;
        }

        AssertWrapper.IsTrue(didFindExpectedScript, $"Found no {expectedScript} in {folderPath}, so the check looked in the wrong place. If the script moved, update this test");

        return codeByScriptPath;
    }

    private static bool IsInAnyScript(Dictionary<string, string> codeByScriptPath, string call)
    {
        foreach (var code in codeByScriptPath.Values)
        {
            if (code.Contains(call))
            {
                return true;
            }
        }

        return false;
    }
}
