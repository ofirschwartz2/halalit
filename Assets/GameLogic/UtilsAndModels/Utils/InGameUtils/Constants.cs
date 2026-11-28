public class Constants
{
    // Attacks
    public const float BOOMERANG_OUT_OF_HALALIT_TIME = 0.1f;

    // Knockback
    public const float ANGULAR_SPEED_KNOCKBACK_MULTIPLIER = 0.03f;

    // Resources
    public const string RESOURCE_DYNAMIC_BAR_PREFAB = "Prefabs/UI/DynamicBar";
    public const string PROJECTILES_GAME_OBJECT_NAME = "Projectiles";
    public const string WEAPON_GAME_OBJECT_NAME = "Weapon";

    // Health bar
    public const float HEALH_BAR_SCALE = 0.01f;
    public const float HEALTH_BAR_Y_OFFSET_ADITION = 0.5f; 

    // Leaderboards
    public const string HIGHSCORE_LEADERBOARD_ID = "1";
    public const string DAILY_LEADERBOARD_ID = "daily";
    public const int DAILY_LEADERBOARD_PAST_DAYS = 7;

    // Daily replay (#244): 4 samples a second of game time, for at most 30 minutes
    public const float DAILY_TRACK_SAMPLE_SECONDS = 0.25f;
    public const int DAILY_TRACK_MAX_SAMPLES = 7200;

    // Enemies
    public const int MAX_LATCHED_LEECHES = 2;

    // Player names
    public const int PLAYER_NAME_MIN_LENGTH = 3;
    public const int PLAYER_NAME_MAX_LENGTH = 12;
}