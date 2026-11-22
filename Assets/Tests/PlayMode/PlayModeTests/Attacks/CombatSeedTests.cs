using Assets.Tests.PlayMode.PlayModeTests.TestInfra;
using Assets.Utils;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class CombatSeedTests
{
    // OriginalRandomSeed hands out seven seeds before the combat seed, which must stay last
    private const int NUMBER_OF_SEEDS_BEFORE_COMBAT = 7;
    private const int NUMBER_OF_ROLLS = 20;
    private int _currentSeed;

    [SetUp]
    public void SetUp()
    {
        _currentSeed = TestUtils.SetRandomSeed();
        SceneManager.LoadScene(TestUtils.PLAYGROUND_SCENE_NAME);
    }

    [UnityTest]
    public IEnumerator CombatRollsFollowTheRunSeed()
    {
        // GIVEN
        yield return null;
        TestUtils.SetTestMode();
        var expectedCombatRandomGenerator = GetExpectedCombatRandomGenerator(_currentSeed);

        // WHEN
        var combatRandomGenerator = CombatRandomGenerator.GetSeedfulRandomGenerator();

        // THEN
        AssertWrapper.IsTrue(combatRandomGenerator != null, "Combat generator was not seeded when Playground loaded", _currentSeed);

        for (var i = 0; i < NUMBER_OF_ROLLS; i++)
        {
            var expectedRoll = expectedCombatRandomGenerator.GetNumber();
            var actualRoll = combatRandomGenerator.GetNumber();
            AssertWrapper.IsTrue(expectedRoll == actualRoll, $"Combat roll {i} is {actualRoll}, expected {expectedRoll} from the run seed", _currentSeed);
        }
    }

    private SeedfulRandomGenerator GetExpectedCombatRandomGenerator(int runSeed)
    {
        var originalRandomGenerator = new SeedfulRandomGenerator(runSeed);

        for (var i = 0; i < NUMBER_OF_SEEDS_BEFORE_COMBAT; i++)
        {
            originalRandomGenerator.GetNumber();
        }

        return new SeedfulRandomGenerator(originalRandomGenerator.GetNumber());
    }

    [TearDown]
    public void TearDown()
    {
        TestUtils.DestroyAllGameObjects();
        TestTimeController.ResetTimeScale();
    }
}
