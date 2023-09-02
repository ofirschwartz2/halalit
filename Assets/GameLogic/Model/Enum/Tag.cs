using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum Tag
    {
        [Description("Halalit")] 
        HALALIT,
        [Description("Enemy")] 
        ENEMY,
        [Description("Shot")] 
        SHOT,
        [Description("Item")] 
        ITEM,
        [Description("PickupClaw")] 
        PICKUP_CLAW,
        [Description("Asteroid")] 
        ASTEROID,
        [Description("InternalWorld")]
        INTERNAL_WORLD,
        [Description("ExternalWorld")]
        EXTERNAL_WORLD,
        [Description("BarBorder")]
        BAR_BORDEDR,
        [Description("BarFill")]
        BAR_FILL,
    }
}