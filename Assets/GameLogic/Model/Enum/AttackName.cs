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
        LASER_BEAM,
        [Description("Knockback shot")]
        KNOCKBACK_SHOT,
    }
}
