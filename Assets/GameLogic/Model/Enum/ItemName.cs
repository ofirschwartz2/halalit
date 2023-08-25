using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum ItemName
    {
        // Attacks
        [Description("Ball shot item")]
        BALL_SHOT = AttackName.BALL_SHOT,
        [Description("Laser beam item")]
        LASER_BEAM = AttackName.LASER_BEAM,

        // Upgrades
        [Description("Fire rate")]
        FIRE_RATE,

        // Utilities
        [Description("Nitro fuel")]
        NITRO_FUEL
    }
}
