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
        LASER_BEAM_SHOT = AttackName.LASER_BEAM_SHOT,
        [Description("Knockback item")]
        KNOCKBACK_SHOT = AttackName.KNOCKBACK_SHOT,
        [Description("Boomerang item")]
        BOOMERANG_SHOT = AttackName.BOOMERANG_SHOT,
        [Description("Mirror Ball shot item")]
        MIRROR_BALL_SHOT = AttackName.MIRROR_BALL_SHOT,
        [Description("Sword item")]
        SWORD = AttackName.SWORD,
        [Description("Blast shot item")] 
        BLAST_SHOT = AttackName.BLAST_SHOT,
        [Description("Granade shot item")]
        GRANADE_SHOT = AttackName.GRANADE_SHOT,
        [Description("Shotgun shot item")]
        SHOTGUN_SHOT = AttackName.SHOTGUN_SHOT,
        [Description("Spikes item")]
        SPIKES = AttackName.SPIKES,

        // Upgrades
        [Description("Halalit max HP")]
        HALALIT_VITALITY,
        [Description("Halalit defence")]
        HALALIT_IMMUNITY,
        [Description("Halalit being affected by knockbacks")]
        HALALIT_VIGOR,
        [Description("Claw rope length")]
        CLAW_RANGE,
        [Description("Claw speed")]
        CLAW_AGILITY,
        [Description("Claw not stamble by collisions")]
        CLAW_STABILITY,
        [Description("Weapon fire rate")]
        WEAPON_STAMINA,
        [Description("Weapon shakeness")]
        WEAPON_DEXTERITY,
        [Description("Weapon movement drag")]
        WEAPON_STRENGH,

        // Utilities
        [Description("Nitro fuel")]
        NITRO_FUEL
    }
}
