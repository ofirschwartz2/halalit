using Assets.Enums;
using System.Collections.Generic;

public class ValuableEventArguments : System.EventArgs
{
    public ValuableName Name;

    public ValuableEventArguments(ValuableName name)
    {
        Name = name;
    }
}
