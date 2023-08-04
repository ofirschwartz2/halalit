using Assets.Enums;
using System.Collections.Generic;

public class UpgradeEventArguments : System.EventArgs
{
    public UpgradeEventArguments(UpgradeName name, Dictionary<EventProperty, float> properties)
    {
        Name = name;
        Properties = properties;
    }

    public UpgradeName Name;
    public Dictionary<EventProperty, float> Properties;
}
