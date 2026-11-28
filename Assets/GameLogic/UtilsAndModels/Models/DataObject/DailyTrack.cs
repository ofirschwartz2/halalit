using System;

// Daily replay (#244): one player's ship path and score over a daily run, stored as JSON in Cloud Save.
// JsonUtility reads public fields only, so every field is a public field and lists are arrays. JsonUtility also needs the default constructor
[Serializable]
public class DailyTrack
{
    public int Version;
    public float SampleSeconds;
    // One byte per axis per sample (x, then y), 0 to 255 across the InternalWorld box, as Base64
    public string Positions;
    // The sample at which the score changed, and the new score, pair by pair
    public int[] ScoreSampleIndexes;
    public int[] Scores;
    public float SurvivalSeconds;
}
