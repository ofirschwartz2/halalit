using System;
using System.Collections.Generic;
using Assets.Enums;
using Assets.Utils;
using NUnit.Framework;
using UnityEngine;

public class CombatRollsTests
{
    private const int NUMBER_OF_ROLLS = 20;
    private const float SHOOTING_RANGE = 30f;
    private const float MIN_SPEED = 1f;
    private const float MAX_SPEED = 2f;
    private const int POWER = 10;
    private const float CRITICAL_HIT = 2f;
    private const int LUCK = 50;

    [Test]
    public void SameSeedSameCombatRolls()
    {
        var seed = TestingRandomGenerator.GetNumber();

        var actualRolls = GetCombatRolls(seed);
        var expectedRolls = GetExpectedCombatRolls(seed);

        for (var i = 0; i < expectedRolls.Count; i++)
        {
            AssertWrapper.IsTrue(actualRolls[i] == expectedRolls[i], $"Combat roll {i} is {actualRolls[i]}, expected {expectedRolls[i]} from the seed", seed);
        }
    }

    private List<float> GetCombatRolls(int seed)
    {
        var attacker = new GameObject();

        try
        {
            var attackBehaviour = attacker.AddComponent<AttackBehaviour>();
            attackBehaviour.AttackStats = new AttackStats(ItemRank.COMMON, POWER, CRITICAL_HIT, LUCK, 1f, 1f);

            CombatRandomGenerator.SetInitialSeed(seed);
            var combatRandomGenerator = CombatRandomGenerator.GetSeedfulRandomGenerator();
            var rolls = new List<float>();

            for (var i = 0; i < NUMBER_OF_ROLLS; i++)
            {
                // The game's own calls: critical hit roll inside GetHarm, shotgun spread, pellet speed
                rolls.Add(attackBehaviour.GetHarm());
                rolls.Add(Utils.GetRandomAngleAround(combatRandomGenerator, SHOOTING_RANGE));
                rolls.Add(Utils.GetRandomBetween(combatRandomGenerator, MIN_SPEED, MAX_SPEED));
            }

            return rolls;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(attacker);
        }
    }

    private List<float> GetExpectedCombatRolls(int seed)
    {
        // A second generator with the same seed, drawn in the same order without game code
        var randomGenerator = new SeedfulRandomGenerator(seed);
        var rolls = new List<float>();

        for (var i = 0; i < NUMBER_OF_ROLLS; i++)
        {
            var isCriticalHit = randomGenerator.Range(0, 100) < LUCK;
            rolls.Add(isCriticalHit ? (int)Math.Floor(POWER * CRITICAL_HIT) : POWER);
            rolls.Add(randomGenerator.Range(-SHOOTING_RANGE, SHOOTING_RANGE));
            rolls.Add(randomGenerator.Range(MIN_SPEED, MAX_SPEED));
        }

        return rolls;
    }
}
