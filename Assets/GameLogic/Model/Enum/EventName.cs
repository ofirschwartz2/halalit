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
        [Description("Item drop")]
        ITEM_DROP,
        [Description("New attack switch")]
        NEW_ATTACK_SWITCH,
    }
}
