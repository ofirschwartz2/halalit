using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum ItemName
    {
        // Weapons
        [Description("Ball shot")]
        BALL_SHOT,
        [Description("Laser beam")]
        LASER_BEAM,

        // Upgrades
        [Description("Fire rate")]
        FIRE_RATE,

        // Utilities
        [Description("Nitro fuel")]
        NITRO_FUEL
    }
}
