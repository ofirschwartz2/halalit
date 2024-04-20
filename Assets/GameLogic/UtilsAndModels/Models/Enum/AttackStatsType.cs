using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum AttackStatsType
    {
        [Description("Power")]
        POWER,
        [Description("Critical Hit")]
        CRITICAL_HIT,
        [Description("Luck")]
        LUCK,
        [Description("Rate")]
        RATE,
        [Description("Weight")]
        WEIGHT
    }
}

