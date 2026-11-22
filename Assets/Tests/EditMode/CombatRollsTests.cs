using System.Collections.Generic;
using Assets.Utils;
using NUnit.Framework;

public class CombatRollsTests
{
    private const int NUMBER_OF_ROLLS = 20;
    private const float SHOOTING_RANGE = 30f;
    private const float MIN_SPEED = 1f;
    private const float MAX_SPEED = 2f;

    [Test]
    public void SameSeedSameCombatRolls()
    {
        var seed = TestingRandomGenerator.GetNumber();

        var firstRolls = GetCombatRolls(seed);
        var secondRolls = GetCombatRolls(seed);

        for (var i = 0; i < firstRolls.Count; i++)
        {
            AssertWrapper.IsTrue(firstRolls[i] == secondRolls[i], $"Combat roll {i} is {firstRolls[i]} in the first run and {secondRolls[i]} in the second", seed);
        }
    }

    private List<float> GetCombatRolls(int seed)
    {
        CombatRandomGenerator.SetInitialSeed(seed);
        var combatRandomGenerator = CombatRandomGenerator.GetSeedfulRandomGenerator();
        var rolls = new List<float>();

        for (var i = 0; i < NUMBER_OF_ROLLS; i++)
        {
            // The same calls the game makes: critical hit, shotgun spread, pellet speed
            rolls.Add(combatRandomGenerator.Range(0, 100));
            rolls.Add(Utils.GetRandomAngleAround(combatRandomGenerator, SHOOTING_RANGE));
            rolls.Add(Utils.GetRandomBetween(combatRandomGenerator, MIN_SPEED, MAX_SPEED));
        }

        return rolls;
    }
}
