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
        SWORD,
        [Description("Blast shot")]
        BLAST_SHOT,
        [Description("Granade shot")]
        GRANADE_SHOT,
        [Description("Shotgun shot")]
        SHOTGUN_SHOT,
        [Description("Spikes")]
        SPIKES
    }
}
