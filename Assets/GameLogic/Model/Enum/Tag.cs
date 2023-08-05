using System.ComponentModel;

namespace Assets.Enums
{
    public enum Tag
    {
        [Description("Halalit")] 
        HALALIT,
        [Description("Enemy")] 
        ENEMY,
        [Description("Shot")] 
        SHOT,
        [Description("Gun")] 
        GUN,
        [Description("Item")] 
        ITEM,
        [Description("PickupClaw")] 
        PICKUP_CLAW,
        [Description("Asteroid")] 
        ASTEROID,
        [Description("Background")]
        BACKGROUND
    }
}