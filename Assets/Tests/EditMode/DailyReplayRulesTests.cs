using System.Collections.Generic;
using NUnit.Framework;

public class DailyReplayRulesTests
{

    [Test]
    public void HighestScoreWins()
    {
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 120, 300f),
            CreateEntry("Ofir", 450, 90f),
            CreateEntry("Yoni", 300, 600f)
        };

        AssertWrapper.AreEqual(1, DailyReplayRules.GetWinnerIndex(entries), "The highest score was not crowned");
    }

    [Test]
    public void TieGoesToTheLongerSurvival()
    {
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 450, 100f),
            CreateEntry("Ofir", 450, 250f)
        };

        AssertWrapper.AreEqual(1, DailyReplayRules.GetWinnerIndex(entries), "A tie on score did not go to the longer survival");
    }

    [Test]
    public void FullTieGoesToTheEarlierEntry()
    {
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 450, 100f),
            CreateEntry("Ofir", 450, 100f),
            CreateEntry("Yoni", 450, 100f)
        };

        AssertWrapper.AreEqual(0, DailyReplayRules.GetWinnerIndex(entries), "A tie on score and survival did not go to the earlier entry");
    }

    [Test]
    public void NobodyPlayedMeansNoWinner()
    {
        var entries = new List<DailyReplayEntry>();

        AssertWrapper.AreEqual(-1, DailyReplayRules.GetWinnerIndex(entries), "A day nobody played had a winner");
        AssertWrapper.AreEqual(string.Empty, DailyReplayRules.GetWinnerLine(entries), "A day nobody played had a winner line");
        AssertWrapper.AreEqual(string.Empty, DailyReplayRules.GetResultsText(entries), "A day nobody played had results");
    }

    [Test]
    public void WinnerWithoutATrackStillTopsTheResults()
    {
        // A player on an older build has no track, so no ship, but the list's score still counts
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 120, 300f),
            CreateEntryWithoutTrack("Ofir", 450)
        };

        AssertWrapper.AreEqual(1, DailyReplayRules.GetWinnerIndex(entries), "The highest score was not crowned because it had no track");
        AssertWrapper.AreEqual("Winner: Ofir", DailyReplayRules.GetWinnerLine(entries), "Wrong winner line for a winner without a track");
        AssertWrapper.AreEqual("1. Ofir  450  no replay\n2. Dana  120  5:00", DailyReplayRules.GetResultsText(entries), "A winner without a track did not top the results");
    }

    [Test]
    public void ResultsAreInCrownOrder()
    {
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 120, 75.4f),
            CreateEntry("Ofir", 450, 600f),
            CreateEntry("Yoni", 120, 200f)
        };

        AssertWrapper.AreEqual("1. Ofir  450  10:00\n2. Yoni  120  3:20\n3. Dana  120  1:15", DailyReplayRules.GetResultsText(entries), "The results lines are wrong");
    }

    [Test]
    public void Clock()
    {
        AssertWrapper.AreEqual("0:00", DailyReplayRules.FormatClock(0f), "Wrong clock at 0 seconds");
        AssertWrapper.AreEqual("1:15", DailyReplayRules.FormatClock(75.4f), "Wrong clock at 75.4 seconds");
        AssertWrapper.AreEqual("10:00", DailyReplayRules.FormatClock(600f), "Wrong clock at 600 seconds");
        AssertWrapper.AreEqual("0:00", DailyReplayRules.FormatClock(-1f), "Wrong clock before the start");
    }

    [Test]
    public void OutText()
    {
        AssertWrapper.AreEqual("450  1:15", DailyReplayRules.GetOutText(450, 75.4f), "Wrong text under a ship that is out");
    }

    [Test]
    public void ShipFliesUntilItsPlayerDied()
    {
        var entry = CreateEntry("Dana", 120, 30f);

        AssertWrapper.IsTrue(DailyReplayRules.IsAlive(entry, 0f), "A ship was out at the start");
        AssertWrapper.IsTrue(DailyReplayRules.IsAlive(entry, 29.9f), "A ship was out before its player died");
        AssertWrapper.IsFalse(DailyReplayRules.IsAlive(entry, 30f), "A ship still flew when its player died");
        AssertWrapper.IsFalse(DailyReplayRules.IsAlive(entry, 31f), "A ship still flew after its player died");
    }

    [Test]
    public void PlayerWithoutATrackNeverFlies()
    {
        AssertWrapper.IsFalse(DailyReplayRules.IsAlive(CreateEntryWithoutTrack("Dana", 120), 0f), "A player without a track flew");
    }

    [Test]
    public void ReplayEndsWhenTheLastShipIsOut()
    {
        var entries = new List<DailyReplayEntry>
        {
            CreateEntry("Dana", 120, 30f),
            CreateEntry("Ofir", 450, 2400f),
            CreateEntryWithoutTrack("Yoni", 300)
        };

        AssertWrapper.AreEqual(2400f, DailyReplayRules.GetEndSeconds(entries), "The replay did not end with the longest survival, past the 30-minute cap");
        AssertWrapper.AreEqual(0f, DailyReplayRules.GetEndSeconds(new List<DailyReplayEntry>()), "A day with no tracks did not end at 0");
    }

    [Test]
    public void FullTieKeepsListOrderForTwentyPlayers()
    {
        // 20 is the most a replay shows. List.Sort is not stable above 16 items, so 3 tied players cannot show that the list order decides last
        var entries = new List<DailyReplayEntry>();
        for (int i = 0; i < 20; i++)
        {
            entries.Add(CreateEntry("Player" + i, 450, 100f));
        }

        var ranking = DailyReplayRules.GetRanking(entries);

        for (int place = 0; place < entries.Count; place++)
        {
            AssertWrapper.AreEqual(place, ranking[place], $"On a full tie of 20 players, place {place + 1} went to entry {ranking[place]}, not to entry {place}");
        }
    }

    [Test]
    public void ScoreTieGoesToTheShipOverAPlayerWithoutATrack()
    {
        // A player without a track counts as surviving 0 seconds, so being earlier in the list does not win the tie
        var entries = new List<DailyReplayEntry>
        {
            CreateEntryWithoutTrack("Dana", 450),
            CreateEntry("Ofir", 450, 1f)
        };

        AssertWrapper.AreEqual(1, DailyReplayRules.GetWinnerIndex(entries), "A score tie went to the player without a track");
    }

    [Test]
    public void RankingFollowsTheCrownRule()
    {
        // Seeded: a failure prints the seed. To replay it, pass the seed to GetSeed
        int seed = GetSeed();
        var random = new System.Random(seed);
        var entries = new List<DailyReplayEntry>();
        float longestSurvival = 0f;

        // Few distinct scores and survival times, so ties on score and on both happen often
        for (int i = 0; i < 20; i++)
        {
            int score = random.Next(0, 4) * 100;
            int survivalChoice = random.Next(0, 3);

            if (survivalChoice == 0)
            {
                entries.Add(CreateEntryWithoutTrack("Player" + i, score));
            }
            else
            {
                float survival = survivalChoice * 30f;
                entries.Add(CreateEntry("Player" + i, score, survival));
                longestSurvival = System.Math.Max(longestSurvival, survival);
            }
        }

        var ranking = DailyReplayRules.GetRanking(entries);

        AssertWrapper.AreEqual(entries.Count, ranking.Count, "The ranking does not hold every player", seed);
        AssertWrapper.AreEqual(entries.Count, new HashSet<int>(ranking).Count, "The ranking holds a player twice", seed);
        AssertWrapper.AreEqual(ranking[0], DailyReplayRules.GetWinnerIndex(entries), "The winner is not first in the ranking", seed);
        AssertWrapper.AreEqual(longestSurvival, DailyReplayRules.GetEndSeconds(entries), "The replay does not end with the longest survival", seed);

        for (int place = 1; place < ranking.Count; place++)
        {
            var ahead = entries[ranking[place - 1]];
            var behind = entries[ranking[place]];
            float aheadSurvival = DailyReplayRules.GetSurvivalSeconds(ahead);
            float behindSurvival = DailyReplayRules.GetSurvivalSeconds(behind);

            bool isInCrownOrder = ahead.Score > behind.Score
                || (ahead.Score == behind.Score && aheadSurvival > behindSurvival)
                || (ahead.Score == behind.Score && aheadSurvival == behindSurvival && ranking[place - 1] < ranking[place]);

            AssertWrapper.IsTrue(isInCrownOrder, $"Place {place} ({ahead.PlayerName}, {ahead.Score}, {aheadSurvival} s) is not ahead of place {place + 1} ({behind.PlayerName}, {behind.Score}, {behindSurvival} s) by the crown rule", seed);
        }
    }

    [Test]
    public void DeathAtTheStartNeverFlies()
    {
        AssertWrapper.IsFalse(DailyReplayRules.IsAlive(CreateEntry("Dana", 0, 0f), 0f), "A ship whose player died at 0 seconds flew");
    }

    private static int GetSeed(int seed = 0)
    {
        return seed != 0 ? seed : System.Environment.TickCount;
    }

    private static DailyReplayEntry CreateEntry(string playerName, int score, float survivalSeconds)
    {
        var entry = CreateEntryWithoutTrack(playerName, score);
        entry.Track = new DailyTrack
        {
            Version = 1,
            SampleSeconds = Constants.DAILY_TRACK_SAMPLE_SECONDS,
            Positions = string.Empty,
            ScoreSampleIndexes = new int[0],
            Scores = new int[0],
            SurvivalSeconds = survivalSeconds
        };
        return entry;
    }

    private static DailyReplayEntry CreateEntryWithoutTrack(string playerName, int score)
    {
        return new DailyReplayEntry
        {
            PlayerName = playerName,
            Score = score,
            IsPlayer = false,
            Track = null
        };
    }
}
