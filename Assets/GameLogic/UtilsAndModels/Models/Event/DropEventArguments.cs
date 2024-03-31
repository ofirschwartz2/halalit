using Assets.Enums;
using UnityEngine;

public class DropEventArguments : System.EventArgs
{
    public Dropper Dropper;
    public Vector2 DropPosition;
    public Vector2 DropForce;
    public ValuableName ValuableName; // TODO: Merge With Item Names

    public DropEventArguments(Dropper dropper, Vector2 dropPosition, Vector2 dropForce)
    {
        Dropper = dropper;
        DropPosition = dropPosition;
        DropForce = dropForce;
    }

    public DropEventArguments(Dropper dropper, Vector2 dropPosition, Vector2 dropForce, ValuableName valuableName)
    {
        Dropper = dropper;
        DropPosition = dropPosition;
        DropForce = dropForce;
        ValuableName = valuableName;
    }
}