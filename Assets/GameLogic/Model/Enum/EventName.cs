using System.ComponentModel;

namespace Assets.Enums
{
    public enum EventName
    {
        [Description("Player upgrade pickup")]
        PLAYER_UPGRADE_PICKUP,
        [Description("Player attack item pickup")]
        PLAYER_ATTACK_ITEM_PICKUP,
        [Description("Asteroid destruction")]
        ASTEROID_DESTRUCTION,
        [Description("New item drop")]
        NEW_ITEM_DROP,
        [Description("No stock")]
        NO_STOCK,
        [Description("New attack switch")]
        NEW_ATTACK_SWITCH,
    }
}
