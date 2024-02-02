using Assets.Enums;
using System.Collections.Generic;

public class ItemEventArguments : System.EventArgs
{
    public ItemName Name;
    public IItemStats ItemStats;
    public Dictionary<EventProperty, float> Properties; // TODO (refactor): delete this and use only the itemStats

    public ItemEventArguments(ItemName name, IItemStats itemStats)
    {
        Name = name;
        ItemStats = itemStats;
    }

    public ItemEventArguments(ItemName name, Dictionary<EventProperty, float> properties)
    {
        Name = name;
        Properties = properties;
    }
}
