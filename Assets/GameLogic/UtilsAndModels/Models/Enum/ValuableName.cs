using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum ValuableName
    {
        [Description("Silver")]
        SILVER,
        [Description("Gold")]
        GOLD,
        [Description("Diamond")]
        DIAMOND
    }
}
