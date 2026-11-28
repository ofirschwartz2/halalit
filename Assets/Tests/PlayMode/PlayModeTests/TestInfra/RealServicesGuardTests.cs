using Assets.Enums;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// No test may sign in for real or post a score to the friends' leaderboard (#307). PlayModeServicesSetUp puts PlayerStats on fakes
// before the first test. If these tests fail, the set-up fixture did not run: do not run the full suite, because its deaths could post for real
public class RealServicesGuardTests
{
    private const int MAX_FRAMES_TO_SAVE = 10;

    private int _currentSeed;
    private bool _didLoadPlayground;

    // Only reads, so it is safe to run even when the set-up fixture did not run
    [Test]
    public void EveryTestStartsOnTheFakeSignIn()
    {
        AssertWrapper.IsFalse(PlayerStats.IsSignInReal(), "PlayerStats is on the real sign-in, so the set-up fixture did not run. Do not run the full suite: a death could post a test score to the friends' leaderboard");
    }

    // Only the seed: switching to the fakes here would also run before EveryTestStartsOnTheFakeSignIn, which could then never fail
    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
    }

    // Passes both before and after Ofir wires the results card: without it the death loads the menu, which saves on the fakes;
    // with it the card saves through PlayerStats on the fakes
    [UnityTest]
    public IEnumerator DeathInAPlaygroundTestUsesOnlyTheFakes()
    {
        // GIVEN
        // A daily of today with a score above 0, so a real save would reach both leaderboards
        TestServices.UseFakes();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
        _didLoadPlayground = true;
        yield return null;
        TestUtils.SetTestMode();
        yield return null;
        yield return TestUtils.CollectValuable(ValuableName.SILVER);
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();

        // WHEN
        TestUtils.KillHalalit();

        for (int i = 0; i < MAX_FRAMES_TO_SAVE && TestServices.SubmitCalls == 0; i++)
        {
            yield return null;
        }

        // THEN
        AssertWrapper.GreaterOrEqual(TestServices.SubmitCalls, 1, "The death's score never reached the fake leaderboard, so it may have gone somewhere else", _currentSeed);
        AssertWrapper.IsFalse(Authentication.AreServicesStarted(), "The death started Unity Services, so it may have signed in or saved for real", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        // Only after the Playground test, which always calls SetTestMode: ResetTimeScale restores the saved scale, and that is 0 if none was saved yet
        if (_didLoadPlayground)
        {
            TestUtils.DestroyAllGameObjects();
            TestTimeController.ResetTimeScale();
            _didLoadPlayground = false;
        }

        TestServices.UseFakes();
        PlayerStats.SetDefaultPlayerStatsValues();
    }
}
