using System;
using System.ComponentModel;

namespace Assets.Enums
{
    // Scenes store these numbers. Never change or reuse one; append new members with the next number.
    [Serializable]
    public enum ItemName
    {
        // Attacks
        [Description("Ball shot item")]
        BALL_SHOT = 0,
        [Description("Laser beam item")]
        LASER_BEAM = 1,
        [Description("Knockback wave item")]
        KNOCKBACK_WAVE = 2,
        [Description("Boomerang item")]
        BOOMERANG = 3,
        [Description("Mirror Ball shot item")]
        MIRROR_BALL_SHOT = 4,
        [Description("Sword item")]
        SWORD = 5,
        [Description("Blast shot item")] 
        BLAST_SHOT = 6,
        [Description("Grenade item")]
        GRENADE = 7,
        [Description("Shotgun item")]
        SHOTGUN = 8,
        [Description("Spikes item")]
        SPIKES = 9,

        // Upgrades
        [Description("Halalit max HP")]
        HALALIT_VITALITY = 10,
        [Description("Halalit defence")]
        HALALIT_IMMUNITY = 11,
        [Description("Halalit being affected by knockbacks")]
        HALALIT_VIGOR = 12,
        [Description("Claw rope length")]
        CLAW_RANGE = 13,
        [Description("Claw speed")]
        CLAW_AGILITY = 14,
        [Description("Claw not stamble by collisions")]
        CLAW_STABILITY = 15,
        [Description("Weapon fire rate")]
        WEAPON_STAMINA = 16,
        [Description("Weapon shakeness")]
        WEAPON_DEXTERITY = 17,
        [Description("Weapon movement drag")]
        WEAPON_STRENGH = 18,

        [Description("ItemBase")] 
        ITEM_BASE = 19,
        
        // Utilities
        [Description("Nitro fuel")]
        NITRO_FUEL = 20,
        [Description("Pushing magnet utility")]
        PUSHING_MAGNET = 21,
        [Description("Pulling magnet utility")]
        PULLING_MAGNET = 22,

        // Attacks added after the pin
        [Description("Grapple shot item")]
        GRAPPLE_SHOT = 23,
        [Description("Swap shot item")]
        SWAP_SHOT = 24,
        [Description("Decoy ship item")]
        DECOY_SHIP = 25,
        [Description("Coin toss item")]
        COIN_TOSS = 26,
        [Description("Bank shot item")]
        BANK_SHOT = 27
    }
}
