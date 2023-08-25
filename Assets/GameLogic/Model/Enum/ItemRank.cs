using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum ItemRank
    {
        [Description("Common")]
        COMMON,
        [Description("Uncommon")]
        UNCOMMON,
        [Description("Rare")]
        RARE,
        [Description("Exclusive")]
        EXCLUSIVE,
        [Description("Epic")]
        EPIC,
        [Description("Legendary")]
        LEGENDARY,
    }
}
