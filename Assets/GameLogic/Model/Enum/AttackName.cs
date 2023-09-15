using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum AttackName
    {
        [Description("Ball shot")]
        BALL_SHOT,
        [Description("Laser beam")]
        LASER_BEAM_SHOT,
        [Description("Knockback shot")]
        KNOCKBACK_SHOT,
        [Description("Boomerang shot")]
        BOOMERANG_SHOT,
        [Description("Mirror Ball shot")]
        MIRROR_BALL_SHOT,
        [Description("Sword")]
        SWORD
    }
}
