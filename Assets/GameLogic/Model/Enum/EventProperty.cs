using System.ComponentModel;

namespace Assets.Enums
{
    public enum EventProperty
    {
        [Description("Halalit HP addition")]
        HALALIT_HP_ADDITION,
        [Description("Halalit defence multiplier subtraction")]
        HALALIT_DEFENCE_MULTIPLIER_SUBTRUCTION,
        [Description("Halalit vigor multiplier subtraction")]
        HALALIT_VIGOR_MULTIPLIER_SUBTRUCTION,
        [Description("Claw range addition")]
        CLAW_RANGE_ADDITION,
        [Description("Claw speed multiplier addition")]
        CLAW_SPEED_MULTIPLIER_ADDITION,
        [Description("Claw stumble multiplier subtruction")]
        CLAW_STUMBLE_MULTIPLIER_SUBTRUCTION,
        [Description("Weapon shake multiplier subtruction")]
        WEAPON_SHAKE_MULTIPLIER_SUBTRUCTION,
        [Description("Weapon cooldown multiplier subtruction")]
        WEAPON_COOLDOWN_MULTIPLIER_SUBTRUCTION,
        [Description("Weapon movement drag multiplier subtruction")]
        WEAPON_MOVEMENT_DRAG_MULTIPLIER_SUBTRUCTION

    }
}
