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
        [Description("Knockback wave item")]
        KNOCKBACK_WAVE = AttackName.KNOCKBACK_WAVE,
        [Description("Boomerang item")]
        BOOMERANG = AttackName.BOOMERANG,
        [Description("Mirror Ball shot item")]
        MIRROR_BALL_SHOT = AttackName.MIRROR_BALL_SHOT,
        [Description("Sword item")]
        SWORD = AttackName.SWORD,
        [Description("Blast shot item")] 
        BLAST_SHOT = AttackName.BLAST_SHOT,
        [Description("Grenade item")]
        GRENADE = AttackName.GRENADE,
        [Description("Shotgun item")]
        SHOTGUN = AttackName.SHOTGUN,
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

        [Description("ItemBase")] 
        ITEM_BASE,
        
        // Utilities
        [Description("Nitro fuel")]
        NITRO_FUEL
    }
}
