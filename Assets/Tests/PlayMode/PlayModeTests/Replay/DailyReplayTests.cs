using System;
using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Daily replay (#244). This test builds the viewer on its own, in the Playground, to check the unwired case.
// The wired viewer and its faked day load are tested through the Leaderboard scene in LeaderBoardTests (#312), once Ofir has wired it
public class DailyReplayTests
{
    private const string NOT_WIRED_WARNING = "DailyReplay: Panel, Arena, Ship Prefab, Clock Text, Status Text or Results Text is not wired in the Leaderboard scene, so Watch does nothing.";

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator UnwiredReplayDoesNothing()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();

        // WHEN
        // Until Ofir wires the Leaderboard scene, the viewer has none of its fields. AddComponent runs its Awake at once
        LogAssert.Expect(LogType.Warning, NOT_WIRED_WARNING);
        var replay = new GameObject("DailyReplay").AddComponent<DailyReplay>();

        // THEN
        AssertWrapper.IsFalse(replay.enabled, "An unwired DailyReplay stayed on, so Awake did not see that it is not wired", _currentSeed);

        // Watch and Back on an unwired viewer must not throw. An exception here fails the test
        replay.Open("version", new DateTime(2026, 11, 27));
        replay.Close();
        replay.Open("version", new DateTime(2026, 11, 27));
        yield return null;

        AssertWrapper.IsFalse(replay.enabled, "An unwired DailyReplay turned on after Open", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
