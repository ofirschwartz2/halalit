using Assets.Enums;
using System.Collections.Generic;

public class UpgradeEventArguments : System.EventArgs
{
    public ItemName Name;
    public Dictionary<EventProperty, float> Properties;

    public UpgradeEventArguments(ItemName name, Dictionary<EventProperty, float> properties)
    {
        Name = name;
        Properties = properties;
    }
}
