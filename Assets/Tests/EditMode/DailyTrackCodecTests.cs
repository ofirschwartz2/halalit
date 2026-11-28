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
    public void NoJsonMeansNoTrack()
    {
        AssertWrapper.IsTrue(DailyTrackCodec.FromJson(null) == null, "A missing track was read as a track");
        AssertWrapper.IsTrue(DailyTrackCodec.FromJson(string.Empty) == null, "An empty track was read as a track");
    }
}
