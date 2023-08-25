using Assets.Enums;
using System.Collections.Generic;

public class ItemEventArguments : System.EventArgs
{
    public ItemName Name;
    public Dictionary<EventProperty, float> Properties;

    public ItemEventArguments(ItemName name, Dictionary<EventProperty, float> properties)
    {
        Name = name;
        Properties = properties;
    }
}
