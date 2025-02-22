using System;
using System.ComponentModel;

namespace Assets.Enums
{
    [Serializable]
    public enum InternalItemTag
    {
        [Description("ItemBase")] 
        ITEM_BASE,
        [Description("NitroFuel")] 
        NITRO_FUEL
    }
}