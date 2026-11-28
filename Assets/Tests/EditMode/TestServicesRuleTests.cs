using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// No test may sign in for real or post a score to the friends' leaderboard (#307). EditModeServicesSetUp puts PlayerStats on fakes
// before the first test, and only TestServices may put the real services back
public class TestServicesRuleTests
{
    private const string TESTS_FOLDER = "Tests";
    private const string TEST_SERVICES_SCRIPT = nameof(TestServices) + ".cs";
    // Built from two parts, so this file does not find itself
    private const string RESET_SERVICES_CALL = "PlayerStats." + "ResetServices(";

    // Only reads, so it is safe to run even when the set-up fixture did not run
    [Test]
    public void EveryTestStartsOnTheFakeSignIn()
    {
        AssertWrapper.IsFalse(PlayerStats.IsSignInReal(), "PlayerStats is on the real sign-in, so the set-up fixture did not run. Do not run the full suite: a test could sign in or post a score for real");
    }

    [Test]
    public void OnlyTestServicesPutsTheRealServicesBack()
    {
        // A test that puts the real services back in its [TearDown] leaves every later test on them, so its deaths could post for real
        var testsFolder = Path.Combine(Application.dataPath, TESTS_FOLDER);
        AssertWrapper.IsTrue(Directory.Exists(testsFolder), $"{testsFolder} does not exist, so nothing was checked");

        var scriptsThatPutTheRealServicesBack = new List<string>();
        var didCheckTestServicesScript = false;

        foreach (var scriptPath in Directory.GetFiles(testsFolder, "*.cs", SearchOption.AllDirectories))
        {
            var isTestServicesScript = Path.GetFileName(scriptPath) == TEST_SERVICES_SCRIPT;
            didCheckTestServicesScript = didCheckTestServicesScript || isTestServicesScript;

            if (!isTestServicesScript && File.ReadAllText(scriptPath).Contains(RESET_SERVICES_CALL))
            {
                scriptsThatPutTheRealServicesBack.Add(scriptPath);
            }
        }

        // TestServices is the one place allowed to call it, so a scan that never reached it looked in the wrong place
        AssertWrapper.IsTrue(didCheckTestServicesScript, $"Found no {TEST_SERVICES_SCRIPT} in {testsFolder}, so the check looked in the wrong place. If the script moved, update this test");
        AssertWrapper.IsTrue(scriptsThatPutTheRealServicesBack.Count == 0, $"These scripts call {RESET_SERVICES_CALL}), which puts the real services back for every later test: {string.Join(", ", scriptsThatPutTheRealServicesBack)}. Call TestServices.UseFakes() instead");
    }
}
