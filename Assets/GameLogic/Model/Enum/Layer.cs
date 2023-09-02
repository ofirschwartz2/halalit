using System.ComponentModel;

namespace Assets.Enums
{
    public enum Layer
    {
        [Description("Default")]
        Default,
        [Description("EnemyShots")]
        EnemyShots,
        [Description("PlayerShots")]
        PlayerShots,
        [Description("Enemies")]
        Enemies,
        [Description("Player")]
        Player,
        [Description("Astroids")]
        Astroids,
        [Description("Items")]
        Items,
    }
}