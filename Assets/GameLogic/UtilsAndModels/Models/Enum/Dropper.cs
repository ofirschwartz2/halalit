using System;

namespace Assets.Enums
{
    [Serializable]
    public enum Dropper // TODO: Move to another nuget
    {
        DEFAULT,
        ASTEROID,
        FOLLOWING_ENEMY_INTERVAL,
        FOLLOWING_ENEMY_RADIUS,
        GREEK_ENEMY,
        LAME_ENEMY,
        SINUS_ENEMY,
        ZIG_ZAG_ENEMY,
        SHOOTING_IN_RANGE_ENEMY,
        SHOOTING_LAZER_ASTERISK_ENEMY,
        SHOOTING_LAZER_RANGE_ENEMY
    }
}
