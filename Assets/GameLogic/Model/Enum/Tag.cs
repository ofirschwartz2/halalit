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
        [Description("KnockbackShot")]
        KNOCKBACK_SHOT,
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
        [Description("TopEdge")]
        TOP_EDGE,
        [Description("BottomEdge")]
        BOTTOM_EDGE,
        [Description("LeftEdge")]
        LEFT_EDGE,
        [Description("RightEdge")]
        RIGHT_EDGE
    }
}