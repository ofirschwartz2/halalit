using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class DailyTrackCodecTests
{
    private const float ONE_STEP = 1f / 255f;

    [Test]
    public void PositionsComeBackWithinOneStep()
    {
        var positions = new List<Vector2>
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0.5f, 0.25f),
            new Vector2(0.123f, 0.987f)
        };

        var track = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 1f));
        var decoded = DailyTrackCodec.GetPositions(track);

        AssertWrapper.AreEqual(positions.Count, decoded.Length, "The track did not come back with one position per sample");
        for (int i = 0; i < positions.Count; i++)
        {
            AssertWrapper.AreEqual(positions[i], decoded[i], $"Position {i} moved by more than one step", null, ONE_STEP);
        }
    }

    [Test]
    public void PositionOutsideTheBoxClampsToItsEdge()
    {
        var positions = new List<Vector2> { new Vector2(-0.4f, 1.7f) };

        var track = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 1f));
        var decoded = DailyTrackCodec.GetPositions(track);

        AssertWrapper.AreEqual(new Vector2(0f, 1f), decoded[0], "A position outside the box was not kept on its edge");
    }

    [Test]
    public void ScoresAndSurvivalComeBack()
    {
        var positions = new List<Vector2> { Vector2.zero, Vector2.one, Vector2.zero };
        var scoreSampleIndexes = new List<int> { 1, 2 };
        var scores = new List<int> { 10, 35 };

        var track = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(positions, scoreSampleIndexes, scores, 42.37f));

        AssertWrapper.IsTrue(track.Version == 1, $"The track's version is {track.Version}, expected 1");
        AssertWrapper.AreEqual(Constants.DAILY_TRACK_SAMPLE_SECONDS, track.SampleSeconds, "The track's sample time did not come back");
        AssertWrapper.IsTrue(track.ScoreSampleIndexes.Length == 2 && track.ScoreSampleIndexes[0] == 1 && track.ScoreSampleIndexes[1] == 2, "The score sample indexes did not come back");
        AssertWrapper.IsTrue(track.Scores.Length == 2 && track.Scores[0] == 10 && track.Scores[1] == 35, "The scores did not come back");
        AssertWrapper.AreEqual(42.37f, track.SurvivalSeconds, "The survival time did not come back", null, 0.001f);
    }

    [Test]
    public void FiveMinutesOfPathFitInFourThousandCharacters()
    {
        // 4,000 is the path budget; score pairs come on top, see BusiestCappedTrackStaysUnder32KiB.
        // 5 minutes at 4 samples a second, with a score change every 10 samples
        var positions = new List<Vector2>();
        var scoreSampleIndexes = new List<int>();
        var scores = new List<int>();
        for (int i = 0; i < 1200; i++)
        {
            positions.Add(new Vector2((i % 100) / 100f, (i % 37) / 37f));
            if (i % 10 == 0)
            {
                scoreSampleIndexes.Add(i);
                scores.Add(i * 5);
            }
        }

        var json = DailyTrackCodec.ToJson(positions, scoreSampleIndexes, scores, 300f);
        var positionsOnly = DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 300f);

        AssertWrapper.IsTrue(positionsOnly.Length <= 4000, $"5 minutes of positions take {positionsOnly.Length} characters of JSON, more than 4,000");
        AssertWrapper.AreEqual(1200, DailyTrackCodec.GetPositions(DailyTrackCodec.FromJson(json)).Length, "5 minutes of track did not come back with 1,200 samples");
    }

    [Test]
    public void DeathAtZeroSecondsEncodesAndDecodes()
    {
        // Sample 0 at the start and the death sample, both at the same moment
        var positions = new List<Vector2> { new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f) };

        var track = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 0f));

        AssertWrapper.IsTrue(track != null, "A death at 0 seconds gave no track");
        AssertWrapper.AreEqual(0f, track.SurvivalSeconds, "A death at 0 seconds did not keep 0 as the survival time");
        AssertWrapper.AreEqual(2, DailyTrackCodec.GetPositions(track).Length, "A death at 0 seconds lost a sample");
        AssertWrapper.IsTrue(track.Scores.Length == 0, "A death at 0 seconds gained a score change");
    }

    [Test]
    public void SamplingIsFourASecondForThirtyMinutes()
    {
        // The replay's speed and the cap both rest on these two numbers. Changing one without the other changes the cap's length
        AssertWrapper.AreEqual(0.25f, Constants.DAILY_TRACK_SAMPLE_SECONDS, "The track is not sampled 4 times a second");
        AssertWrapper.AreEqual(30f * 60f, Constants.DAILY_TRACK_MAX_SAMPLES * Constants.DAILY_TRACK_SAMPLE_SECONDS, "The sample cap is not 30 minutes of track", null, 0.001f);
    }

    [Test]
    public void SampleIsDueEveryQuarterSecond()
    {
        AssertWrapper.IsFalse(DailyTrackCodec.IsSampleDue(4, 0.99f), "Sample 4 was due before 1 second");
        AssertWrapper.IsTrue(DailyTrackCodec.IsSampleDue(4, 1f), "Sample 4 was not due at 1 second");
        AssertWrapper.IsTrue(DailyTrackCodec.IsSampleDue(0, 0f), "Sample 0 was not due at the start");
    }

    [Test]
    public void NoSampleIsDueAtTheCap()
    {
        // Well past the time the next sample would be due
        AssertWrapper.IsFalse(DailyTrackCodec.IsSampleDue(Constants.DAILY_TRACK_MAX_SAMPLES, 3600f), "A sample was due after the 30-minute cap");
    }

    [Test]
    public void SamplesCanBeAddedUpToTheCap()
    {
        AssertWrapper.IsTrue(DailyTrackCodec.CanAddSample(Constants.DAILY_TRACK_MAX_SAMPLES - 1), "The last sample before the cap could not be added");
        AssertWrapper.IsFalse(DailyTrackCodec.CanAddSample(Constants.DAILY_TRACK_MAX_SAMPLES), "A sample could be added at the cap");
    }

    [Test]
    public void SampleIndexOfAMoment()
    {
        // Past the cap, the final score's index lies beyond the last position
        AssertWrapper.IsTrue(DailyTrackCodec.GetSampleIndex(0f) == 0, $"0 seconds gave sample {DailyTrackCodec.GetSampleIndex(0f)}, expected 0");
        AssertWrapper.IsTrue(DailyTrackCodec.GetSampleIndex(1800.1f) == 7201, $"1800.1 seconds gave sample {DailyTrackCodec.GetSampleIndex(1800.1f)}, expected 7201");
    }

    [Test]
    public void ThirtyMinutePathFitsInTwentyThousandCharacters()
    {
        // The recorder stops sampling at the cap. The approach budgets about 19 KB for the capped path
        var positions = new List<Vector2>();
        for (int i = 0; i < Constants.DAILY_TRACK_MAX_SAMPLES; i++)
        {
            positions.Add(new Vector2((i % 100) / 100f, (i % 37) / 37f));
        }

        var json = DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 1800f);

        AssertWrapper.IsTrue(json.Length <= 20000, $"30 minutes of positions take {json.Length} characters of JSON, more than 20,000");
        AssertWrapper.AreEqual(Constants.DAILY_TRACK_MAX_SAMPLES, DailyTrackCodec.GetPositions(DailyTrackCodec.FromJson(json)).Length, "30 minutes of track did not come back with every sample");
    }

    [Test]
    public void BusiestCappedTrackStaysUnder32KiB()
    {
        // Cloud Save allows 5 MiB of public data per player and states no limit for one value.
        // At the cap, with the score changing every 2.5 seconds, a track under 32 KiB leaves room for at least 160 capped dailies
        var positions = new List<Vector2>();
        var scoreSampleIndexes = new List<int>();
        var scores = new List<int>();
        for (int i = 0; i < Constants.DAILY_TRACK_MAX_SAMPLES; i++)
        {
            positions.Add(new Vector2((i % 100) / 100f, (i % 37) / 37f));
            if (i % 10 == 0)
            {
                scoreSampleIndexes.Add(i);
                scores.Add(i * 5);
            }
        }

        var json = DailyTrackCodec.ToJson(positions, scoreSampleIndexes, scores, 1800f);
        var track = DailyTrackCodec.FromJson(json);

        AssertWrapper.IsTrue(json.Length <= 32768, $"The busiest capped track takes {json.Length} characters of JSON, more than 32 KiB");
        AssertWrapper.IsTrue(track.Scores.Length == scores.Count && track.Scores[track.Scores.Length - 1] == scores[scores.Count - 1], "The busiest capped track lost score changes");
    }

    [Test]
    public void PositionHalfwayBetweenTwoSamples()
    {
        var positions = new[] { new Vector2(0f, 0f), new Vector2(0.4f, 0.8f), new Vector2(1f, 1f) };

        // Halfway between sample 1 (0.25 s) and sample 2 (0.5 s)
        var position = DailyTrackCodec.GetPosition(positions, 0.25f, 0.375f);

        AssertWrapper.AreEqual(new Vector2(0.7f, 0.9f), position, "The ship was not halfway between two samples", null, 0.0001f);
    }

    [Test]
    public void PositionBeforeTheStartIsTheFirstSample()
    {
        var positions = new[] { new Vector2(0.2f, 0.3f), new Vector2(1f, 1f) };

        AssertWrapper.AreEqual(new Vector2(0.2f, 0.3f), DailyTrackCodec.GetPosition(positions, 0.25f, -1f), "Before the start, the ship was not on the first sample");
    }

    [Test]
    public void PositionAfterTheLastSampleIsHeld()
    {
        // Past the 30-minute cap the path ends but the run goes on, so the ship stays on the last sample
        var positions = new[] { new Vector2(0f, 0f), new Vector2(0.6f, 0.1f) };

        AssertWrapper.AreEqual(new Vector2(0.6f, 0.1f), DailyTrackCodec.GetPosition(positions, 0.25f, 0.25f), "On the last sample, the ship was elsewhere");
        AssertWrapper.AreEqual(new Vector2(0.6f, 0.1f), DailyTrackCodec.GetPosition(positions, 0.25f, 10f), "After the last sample, the ship did not stay on it");
    }

    [Test]
    public void ScoreBeforeAndAfterAChange()
    {
        // The score became 10 at sample 4 (1 second) and 35 at sample 8 (2 seconds)
        var track = CreateScoreTrack(new[] { 4, 8 }, new[] { 10, 35 }, 5f);

        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(track, 0.99f), "The score changed before its first change");
        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(track, 1f), "The score did not change at its first change");
        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(track, 1.99f), "The score took its second change early");
        AssertWrapper.AreEqual(35, DailyTrackCodec.GetScore(track, 2f), "The score did not take its second change");
    }

    [Test]
    public void ScoreIsReadByTimeNotByPosition()
    {
        // The sample time is part of the track: at 0.5 s a sample, sample 4 is at 2 seconds
        var track = CreateScoreTrack(new[] { 4 }, new[] { 10 }, 5f);
        track.SampleSeconds = 0.5f;

        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(track, 1f), "The score was read by sample index, not by the track's sample time");
        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(track, 2f), "The score did not change at sample 4 times the track's sample time");
    }

    [Test]
    public void ScoreAtTheDeathIsTheFinalScore()
    {
        // A run that died at 1800.1 s, past the 30-minute cap: its final score sits on sample 7201, which is 1800.25 s and beyond the last position
        var track = CreateScoreTrack(new[] { 4, 7201 }, new[] { 10, 900 }, 1800.1f);

        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(track, 1800f), "The final score showed before the death");
        AssertWrapper.AreEqual(900, DailyTrackCodec.GetScore(track, 1800.1f), "At the death, the score was not the run's final score");
        AssertWrapper.AreEqual(900, DailyTrackCodec.GetScore(track, 5000f), "After the death, the score was not the run's final score");
    }

    [Test]
    public void TrackWithoutScoreChangesScoresZero()
    {
        var track = CreateScoreTrack(new int[0], new int[0], 5f);

        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(track, 3f), "A run that never scored showed a score");
        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(track, 10f), "A run that never scored showed a final score");
    }

    [Test]
    public void OneSamplePathStaysOnItsSample()
    {
        // A death at once leaves a path of one sample: the ship must stay there, not read past the end
        var positions = new[] { new Vector2(0.3f, 0.7f) };

        AssertWrapper.AreEqual(positions[0], DailyTrackCodec.GetPosition(positions, 0.25f, -1f), "Before the start, a one-sample ship was elsewhere");
        AssertWrapper.AreEqual(positions[0], DailyTrackCodec.GetPosition(positions, 0.25f, 0f), "At the start, a one-sample ship was elsewhere");
        AssertWrapper.AreEqual(positions[0], DailyTrackCodec.GetPosition(positions, 0.25f, 0.1f), "Between samples, a one-sample ship was elsewhere");
        AssertWrapper.AreEqual(positions[0], DailyTrackCodec.GetPosition(positions, 0.25f, 100f), "Long after the end, a one-sample ship was elsewhere");
    }

    [Test]
    public void PositionWithoutASampleTimeStaysOnTheFirstSample()
    {
        // A damaged track with no sample time must not divide by zero
        var positions = new[] { new Vector2(0.2f, 0.3f), new Vector2(1f, 1f) };

        AssertWrapper.AreEqual(positions[0], DailyTrackCodec.GetPosition(positions, 0f, 5f), "With no sample time, the ship left its first sample");
    }

    [Test]
    public void EmptyTrackHasNoPositions()
    {
        // The viewer shows no ship for a path without positions. This is the part of that it can test without Unity's servers
        var roundTrip = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(new List<Vector2>(), new List<int>(), new List<int>(), 0f));
        var noPositions = CreateScoreTrack(new int[0], new int[0], 5f);
        var nullPositions = CreateScoreTrack(new int[0], new int[0], 5f);
        nullPositions.Positions = null;

        AssertWrapper.AreEqual(0, DailyTrackCodec.GetPositions(roundTrip).Length, "A track recorded with no positions came back with positions");
        AssertWrapper.AreEqual(0, DailyTrackCodec.GetPositions(noPositions).Length, "An empty position string gave positions");
        AssertWrapper.AreEqual(0, DailyTrackCodec.GetPositions(nullPositions).Length, "A missing position string gave positions");
    }

    [Test]
    public void ShipIsOnEachSampleAtItsTime()
    {
        // Seeded: a failure prints the seed. To replay it, pass the seed to GetSeed
        int seed = GetSeed();
        var random = new System.Random(seed);
        var positions = new Vector2[random.Next(2, 50)];
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
        }

        for (int i = 0; i < positions.Length; i++)
        {
            float sampleTime = i * Constants.DAILY_TRACK_SAMPLE_SECONDS;
            AssertWrapper.AreEqual(positions[i], DailyTrackCodec.GetPosition(positions, Constants.DAILY_TRACK_SAMPLE_SECONDS, sampleTime), $"At {sampleTime} s the ship was not on sample {i} of {positions.Length}", seed, 0.0001f);

            if (i < positions.Length - 1)
            {
                float halfwayTime = sampleTime + Constants.DAILY_TRACK_SAMPLE_SECONDS / 2f;
                AssertWrapper.AreEqual((positions[i] + positions[i + 1]) / 2f, DailyTrackCodec.GetPosition(positions, Constants.DAILY_TRACK_SAMPLE_SECONDS, halfwayTime), $"At {halfwayTime} s the ship was not halfway between samples {i} and {i + 1}", seed, 0.0001f);
            }
        }
    }

    [Test]
    public void ScoreWithMissingOrUnpairedChangesDoesNotBreak()
    {
        var missingScores = CreateScoreTrack(null, null, 5f);
        // A damaged track with more scores than sample indexes: only whole pairs count
        var unpaired = CreateScoreTrack(new[] { 4 }, new[] { 10, 35 }, 5f);

        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(missingScores, 3f), "A track without score arrays showed a score");
        AssertWrapper.AreEqual(0, DailyTrackCodec.GetScore(missingScores, 10f), "A track without score arrays showed a final score");
        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(unpaired, 2f), "A track with an unpaired score did not show its paired score");
        AssertWrapper.AreEqual(10, DailyTrackCodec.GetScore(unpaired, 10f), "A track with an unpaired score showed the unpaired score as final");
    }

    private static int GetSeed(int seed = 0)
    {
        return seed != 0 ? seed : System.Environment.TickCount;
    }

    [Test]
    public void TrackThatCannotBeShownHasNoPath()
    {
        // The viewer shows such a player without a ship and counts them as having no track
        var emptyPositions = CreateScoreTrack(new int[0], new int[0], 5f);
        var invalidBase64 = CreateScoreTrack(new int[0], new int[0], 5f);
        invalidBase64.Positions = "not base64!";
        // One byte is half a position, so no whole position
        var oneByte = CreateScoreTrack(new int[0], new int[0], 5f);
        oneByte.Positions = "AA==";

        AssertWrapper.IsTrue(DailyTrackCodec.GetPathOrNull(null) == null, "A missing track gave a path");
        AssertWrapper.IsTrue(DailyTrackCodec.GetPathOrNull(emptyPositions) == null, "A track with no positions gave a path");
        AssertWrapper.IsTrue(DailyTrackCodec.GetPathOrNull(invalidBase64) == null, "A track whose positions are not Base64 gave a path");
        AssertWrapper.IsTrue(DailyTrackCodec.GetPathOrNull(oneByte) == null, "A track with one byte of positions gave a path");
    }

    [Test]
    public void ValidTrackHasAPath()
    {
        var positions = new List<Vector2> { new Vector2(0.5f, 0.5f) };
        var track = DailyTrackCodec.FromJson(DailyTrackCodec.ToJson(positions, new List<int>(), new List<int>(), 0f));

        var path = DailyTrackCodec.GetPathOrNull(track);

        AssertWrapper.IsTrue(path != null, "A valid one-sample track gave no path");
        AssertWrapper.AreEqual(1, path.Length, "A valid one-sample track did not give one position");
    }

    private static DailyTrack CreateScoreTrack(int[] scoreSampleIndexes, int[] scores, float survivalSeconds)
    {
        return new DailyTrack
        {
            Version = 1,
            SampleSeconds = Constants.DAILY_TRACK_SAMPLE_SECONDS,
            Positions = string.Empty,
            ScoreSampleIndexes = scoreSampleIndexes,
            Scores = scores,
            SurvivalSeconds = survivalSeconds
        };
    }

    [Test]
    public void NoJsonMeansNoTrack()
    {
        AssertWrapper.IsTrue(DailyTrackCodec.FromJson(null) == null, "A missing track was read as a track");
        AssertWrapper.IsTrue(DailyTrackCodec.FromJson(string.Empty) == null, "An empty track was read as a track");
    }
}
