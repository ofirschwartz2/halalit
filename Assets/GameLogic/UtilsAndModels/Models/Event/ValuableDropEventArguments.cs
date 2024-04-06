using Assets.Enums;
using UnityEngine;

public class ValuableDropEventArguments : System.EventArgs
{
    public Dropper Dropper;
    public Vector2 DropPosition;
    public Vector2 DropForce;
    public ValuableName ValuableName;

    public ValuableDropEventArguments(Dropper dropper, Vector2 dropPosition, Vector2 dropForce, ValuableName valuableName)
    {
        Dropper = dropper;
        DropPosition = dropPosition;
        DropForce = dropForce;
        ValuableName = valuableName;
    }
}