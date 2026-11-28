using System;
using System.Collections.Generic;
using UnityEngine;

// Daily replay (#244): turns a recorded track into the JSON stored in Cloud Save, and back.
// Positions are 0 to 1 across the InternalWorld box, kept as one byte per axis, so 5 minutes of track is about 3,200 characters
public static class DailyTrackCodec
{
    private const int VERSION = 1;
    private const float BYTE_STEPS = 255f;

    public static string ToJson(IList<Vector2> normalisedPositions, IList<int> scoreSampleIndexes, IList<int> scores, float survivalSeconds)
    {
        var track = new DailyTrack
        {
            Version = VERSION,
            SampleSeconds = Constants.DAILY_TRACK_SAMPLE_SECONDS,
            Positions = EncodePositions(normalisedPositions),
            ScoreSampleIndexes = ToArray(scoreSampleIndexes),
            Scores = ToArray(scores),
            SurvivalSeconds = survivalSeconds
        };

        return JsonUtility.ToJson(track);
    }

    // Null when there is no track
    public static DailyTrack FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }

        return JsonUtility.FromJson<DailyTrack>(json);
    }

    // Decode once and keep the result: each position is 0 to 1 on both axes
    public static Vector2[] GetPositions(DailyTrack track)
    {
        if (string.IsNullOrEmpty(track.Positions))
        {
            return new Vector2[0];
        }

        byte[] bytes = Convert.FromBase64String(track.Positions);
        var positions = new Vector2[bytes.Length / 2];

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = new Vector2(bytes[i * 2] / BYTE_STEPS, bytes[i * 2 + 1] / BYTE_STEPS);
        }

        return positions;
    }

    #region Sampling
    // Sample n is due at n times the sample time, until the 30-minute cap
    public static bool IsSampleDue(int samplesTaken, float elapsedSeconds)
    {
        return CanAddSample(samplesTaken) && elapsedSeconds >= samplesTaken * Constants.DAILY_TRACK_SAMPLE_SECONDS;
    }

    public static bool CanAddSample(int samplesTaken)
    {
        return samplesTaken < Constants.DAILY_TRACK_MAX_SAMPLES;
    }

    // The sample a moment falls on. Past the cap it is beyond the last position, so it is used for score timing only
    public static int GetSampleIndex(float elapsedSeconds)
    {
        return Mathf.CeilToInt(elapsedSeconds / Constants.DAILY_TRACK_SAMPLE_SECONDS);
    }
    #endregion

    private static string EncodePositions(IList<Vector2> normalisedPositions)
    {
        var bytes = new byte[normalisedPositions.Count * 2];

        for (int i = 0; i < normalisedPositions.Count; i++)
        {
            bytes[i * 2] = Quantise(normalisedPositions[i].x);
            bytes[i * 2 + 1] = Quantise(normalisedPositions[i].y);
        }

        return Convert.ToBase64String(bytes);
    }

    // A position outside the box is kept on its edge
    private static byte Quantise(float value)
    {
        return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * BYTE_STEPS);
    }

    private static int[] ToArray(IList<int> values)
    {
        var array = new int[values.Count];
        values.CopyTo(array, 0);
        return array;
    }
}
