using System.ComponentModel;

namespace Assets.Enums
{
    public enum EventName
    {
        [Description("Player upgrade pickup")]
        PLAYER_UPGRADE_PICKUP,
        [Description("Asteroid destruction")]
        ASTEROID_DESTRUCTION,
        [Description("Item drop")]
        ITEM_DROP
    }
}
