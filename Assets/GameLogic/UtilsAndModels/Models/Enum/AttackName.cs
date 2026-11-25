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
        [Description("Knockback wave")]
        KNOCKBACK_WAVE,
        [Description("Boomerang")]
        BOOMERANG,
        [Description("Mirror Ball shot")]
        MIRROR_BALL_SHOT,
        [Description("Sword")]
        SWORD,
        [Description("Blast shot")]
        BLAST_SHOT,
        [Description("Grenade")]
        GRENADE,
        [Description("Shotgun")]
        SHOTGUN,
        [Description("Spikes")]
        SPIKES,
        [Description("Grapple shot")]
        GRAPPLE_SHOT,
        [Description("Swap shot")]
        SWAP_SHOT,
        [Description("Decoy ship")]
        DECOY_SHIP
    }
}
