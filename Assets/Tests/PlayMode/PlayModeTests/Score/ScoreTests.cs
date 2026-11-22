using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class ScoreTests
{

    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator GameOverHandsScoreToPlayerStats()
    {
        // GIVEN
        var numberOfValuables = 3;
        var positions = new List<Vector2>()
        {
            new Vector2(1f, 0f),
            new Vector2(2f, 0f),
            new Vector2(3f, 0f)
        };
        TestUtils.SetTestMode();
        var objectLoader = GameObject.Find(TestUtils.OBJECT_LOADER_NAME).GetComponent<ObjectLoader>();
        List<GameObject> valuables = objectLoader.LoadValuablesInExternalSafeIsland(numberOfValuables);
        yield return null;

        for (int i = 0; i < numberOfValuables; i++)
        {
            TestUtils.SetGameObjectPosition(valuables[i], positions[i]);
        }

        var halalitMovement = TestUtils.GetHalalitMovement();
        var newValuables = TestUtils.GetValuables();
        yield return new WaitForSeconds(1);

        // WHEN
        do
        {
            var rightVector = Vector2.right;
            halalitMovement.TryMove(rightVector.x, rightVector.y, Time.deltaTime);
            newValuables = TestUtils.GetValuables();
            yield return null;

        } while (newValuables.Length != 0);

        // GIVEN
        // A death must not load the real menu in a test run, because the menu signs in for real.
        // The helper takes the menu load off the game over event. It takes effect one frame later.
        TestUtils.UnhookMenuLoadFromGameOver();
        yield return null;

        var score = TestUtils.GetScore();

        // WHEN
        // A first-time player on a daily run dies. Reset, kill, read and reset are one step with no yield.
        // The finally block resets PlayerStats even when the kill throws,
        // so no test score is left behind for a later test or a real menu.
        int? highScore;
        bool isNewHighScore;
        int? dailyScore;
        PlayerStats.SetDefaultPlayerStatsValues();
        PlayerStats.SetStartDaily();
        try
        {
            TestUtils.KillHalalit();
            highScore = PlayerStats._highScore;
            isNewHighScore = PlayerStats._isNewHighScore;
            dailyScore = PlayerStats._dailyScore;
        }
        finally
        {
            PlayerStats.SetDefaultPlayerStatsValues();
        }
        yield return null;

        // THEN
        AssertWrapper.IsFalse(TestUtils.IsGameOver(), "Game Over Loaded The Menu", _currentSeed);
        AssertWrapper.Greater(score, 0, "No Score Collected", _currentSeed);
        AssertWrapper.IsTrue(highScore == score, "High Score Not Set", _currentSeed);
        AssertWrapper.IsTrue(isNewHighScore, "New High Score Not Flagged", _currentSeed);
        AssertWrapper.IsTrue(dailyScore == score, "Daily Score Not Set", _currentSeed);
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}