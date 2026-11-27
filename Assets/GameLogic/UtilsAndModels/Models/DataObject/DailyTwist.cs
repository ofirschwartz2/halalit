using Assets.Enums;

// Daily Twist (#239): one rule for a daily run. Each knob changes one number the game reads at the start of a run.
// A knob at its neutral value keeps the scene's own number: StartHealth 0, SpawnIntervalMultiplier 1, AsteroidMinScale 0.
public class DailyTwist
{
    public readonly DailyTwistName Name;
    public readonly string Title;
    public readonly string Description;
    public readonly int StartHealth;
    public readonly float SpawnIntervalMultiplier;
    public readonly float AsteroidMinScale;

    public DailyTwist(DailyTwistName name, string title, string description, int startHealth, float spawnIntervalMultiplier, float asteroidMinScale)
    {
        Name = name;
        Title = title;
        Description = description;
        StartHealth = startHealth;
        SpawnIntervalMultiplier = spawnIntervalMultiplier;
        AsteroidMinScale = asteroidMinScale;
    }

    public int GetStartHealth(int sceneValue)
    {
        return StartHealth > 0 ? StartHealth : sceneValue;
    }

    public float GetSpawnInterval(float sceneInterval)
    {
        return sceneInterval * SpawnIntervalMultiplier;
    }

    public float GetAsteroidMinScale(float sceneMinScale)
    {
        return AsteroidMinScale > 0 ? AsteroidMinScale : sceneMinScale;
    }

    public string GetDisplayLine()
    {
        return Title + ": " + Description;
    }

    public bool IsNone()
    {
        return Name == DailyTwistName.NONE;
    }
}
